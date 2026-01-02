using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThuVien.Models;

namespace ThuVien.Controllers
{
    public class ThongKeController : Controller
    {
        private Model1 db = new Model1();

        // GET: ThongKe
        public ActionResult Index()
        {
            var thongKe = new
            {
                TongSoSach = db.Saches.Count(),
                TongSoLoaiSach = db.LoaiSaches.Count(),
                TongSoNXB = db.NXBs.Count(),
                TongSoTacGia = db.TacGias.Count(),
                TongSoViTri = db.ViTris.Count(),
                TongSoSinhVien = db.SinhViens.Count(),
                TongSoPhieuMuon = db.PhieuMuons.Count(),
                TongSoUser = db.Users.Count(),
                PhieuMuonChuaTra = db.PhieuMuons.Count(p => p.NgayTra == null || p.NgayTra > DateTime.Now)
            };

            ViewBag.ThongKe = thongKe;
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

