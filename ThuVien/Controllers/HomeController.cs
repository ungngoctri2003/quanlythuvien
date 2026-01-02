using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ThuVien.Models;

namespace ThuVien.Controllers
{
    public class HomeController : Controller
    {
        private Model1 _db = new Model1();
        // GET: Home
        public ActionResult Index()
        {
            if (Session["ID"] != null)
            {
                var viewModel = new DashboardViewModel();

                // Thống kê tổng quan
                viewModel.TotalSach = _db.Saches.Count();
                viewModel.TotalSinhVien = _db.SinhViens.Count();
                viewModel.TotalPhieuMuon = _db.PhieuMuons.Count();
                viewModel.TotalSachDangMuon = _db.Saches.Count(s => s.Muon != null);

                // Thống kê 30 ngày qua
                var date30DaysAgo = DateTime.Now.AddDays(-30);
                viewModel.PhieuMuon30Ngay = _db.PhieuMuons.Count(p => p.NgayMuon.HasValue && p.NgayMuon >= date30DaysAgo);
                
                // Đếm sách mượn trong 30 ngày qua
                var phieuMuonIds30Ngay = _db.PhieuMuons
                    .Where(p => p.NgayMuon.HasValue && p.NgayMuon >= date30DaysAgo)
                    .Select(p => p.ID)
                    .ToList();
                viewModel.SachMuon30Ngay = _db.Saches.Count(s => s.Muon.HasValue && phieuMuonIds30Ngay.Contains(s.Muon.Value));
                
                viewModel.SinhVienMoi30Ngay = _db.SinhViens.Count(s => s.TimeCreate.HasValue && s.TimeCreate >= date30DaysAgo);

                // Sách mới nhất (5 cuốn)
                viewModel.RecentSaches = _db.Saches
                    .OrderByDescending(s => s.TimeCreate)
                    .Take(5)
                    .ToList();

                // Phiếu mượn gần đây (5 phiếu) - Load related entities
                var recentPhieuMuons = _db.PhieuMuons
                    .Include("SinhVien")
                    .Include("Saches")
                    .OrderByDescending(p => p.NgayMuon)
                    .Take(5)
                    .ToList();

                viewModel.RecentPhieuMuons = recentPhieuMuons.Select(p => new PhieuMuonInfo
                {
                    MaMuon = p.MaMuon,
                    TenSinhVien = p.SinhVien != null ? p.SinhVien.HoTen : "N/A",
                    NgayMuon = p.NgayMuon,
                    NgayTra = p.NgayTra,
                    SoSach = p.Saches != null ? p.Saches.Count : 0
                }).ToList();

                // Top sách được mượn nhiều nhất (5 cuốn)
                var topSachMuon = _db.Saches
                    .Where(s => s.Muon != null)
                    .GroupBy(s => new { s.MaSach, s.TenSach })
                    .Select(g => new SachInfo
                    {
                        MaSach = g.Key.MaSach,
                        TenSach = g.Key.TenSach ?? "N/A",
                        SoLuongMuon = g.Count()
                    })
                    .OrderByDescending(s => s.SoLuongMuon)
                    .Take(5)
                    .ToList();
                
                viewModel.TopSachMuon = topSachMuon;

                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //GET: Register

        public ActionResult Register()
        {
            return View();
        }

        //POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User _user)
        {
            if (ModelState.IsValid)
            {
                var check = _db.Users.FirstOrDefault(s => s.Email == _user.Email);
                if (check == null)
                {
                    _user.Password = GetMD5(_user.Password);
                    _db.Configuration.ValidateOnSaveEnabled = false;
                    _db.Users.Add(_user);
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.error = "Email already exists";
                    return View();
                }


            }
            return View();


        }

        public ActionResult Login()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (ModelState.IsValid)
            {


                var f_password = GetMD5(password);
                var data = _db.Users.Where(s => s.Email.Equals(email) && s.Password.Equals(f_password)).ToList();
                if (data.Count() > 0)
                {
                    //add session
                    Session["FullName"] = data.FirstOrDefault().Name;
                    Session["Email"] = data.FirstOrDefault().Email;
                    Session["ID"] = data.FirstOrDefault().ID;
                    return RedirectToAction("Index");

                }
                else
                {
                    ViewBag.error = "Login failed";
                    return RedirectToAction("Login");
                }
            }
            return View();
        }


        //Logout
        public ActionResult Logout()
        {
            Session.Clear();//remove session
            return RedirectToAction("Login");
        }

        //GET: ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //POST: ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.error = "Vui lòng nhập email";
                return View();
            }

            var user = _db.Users.FirstOrDefault(s => s.Email.Equals(email));
            if (user != null)
            {
                // Reset password về mật khẩu mặc định "123456"
                string defaultPassword = "123456";
                user.Password = GetMD5(defaultPassword);
                user.TimeUpdate = DateTime.Now;
                _db.Configuration.ValidateOnSaveEnabled = false;
                _db.Entry(user).State = EntityState.Modified;
                _db.SaveChanges();

                ViewBag.success = "Mật khẩu đã được reset về mặc định: <strong>" + defaultPassword + "</strong>. Vui lòng đăng nhập và đổi mật khẩu sau khi đăng nhập.";
            }
            else
            {
                ViewBag.error = "Email không tồn tại trong hệ thống";
            }

            return View();
        }



        //create a string MD5
        public static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");

            }
            return byte2String;
        }

    }

}
