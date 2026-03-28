using System;
using System.Collections.Generic;

namespace driving_school_management.Models.DTOs
{
    public class HoSoDetailDto
    {
        public int HoSoId { get; set; }
        public string HoTen { get; set; }
        public string SoCmndCccd { get; set; }
        public DateTime? NamSinh { get; set; }
        public string GioiTinh { get; set; }
        public string Sdt { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }

        public string TenHoSo { get; set; }
        public string LoaiHoSo { get; set; }
        public DateTime? NgayDangKy { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
        public string TenHang { get; set; }

        public string HieuLuc { get; set; }
        public DateTime? ThoiHan { get; set; }
        public string KhamMat { get; set; }
        public string HuyetAp { get; set; }
        public decimal? ChieuCao { get; set; }
        public decimal? CanNang { get; set; }

        public int SoThangConLai { get; set; }
        public int SoNgayConLai { get; set; }

        public List<string> Images { get; set; } = new List<string>();
    }
}