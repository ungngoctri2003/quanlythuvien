using System;
using System.Collections.Generic;

namespace ThuVien.Models
{
    public class DashboardViewModel
    {
        // Thống kê tổng quan
        public int TotalSach { get; set; }
        public int TotalSinhVien { get; set; }
        public int TotalPhieuMuon { get; set; }
        public int TotalSachDangMuon { get; set; }

        // Thống kê 30 ngày qua
        public int PhieuMuon30Ngay { get; set; }
        public int SachMuon30Ngay { get; set; }
        public int SinhVienMoi30Ngay { get; set; }

        // Sách mới nhất
        public List<Sach> RecentSaches { get; set; }

        // Phiếu mượn gần đây
        public List<PhieuMuonInfo> RecentPhieuMuons { get; set; }

        // Sách đang được mượn nhiều nhất
        public List<SachInfo> TopSachMuon { get; set; }

        public DashboardViewModel()
        {
            RecentSaches = new List<Sach>();
            RecentPhieuMuons = new List<PhieuMuonInfo>();
            TopSachMuon = new List<SachInfo>();
        }
    }

    public class PhieuMuonInfo
    {
        public string MaMuon { get; set; }
        public string TenSinhVien { get; set; }
        public DateTime? NgayMuon { get; set; }
        public DateTime? NgayTra { get; set; }
        public int SoSach { get; set; }
    }

    public class SachInfo
    {
        public string MaSach { get; set; }
        public string TenSach { get; set; }
        public int SoLuongMuon { get; set; }
    }
}

