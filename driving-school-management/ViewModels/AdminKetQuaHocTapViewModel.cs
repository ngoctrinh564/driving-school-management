namespace driving_school_management.ViewModels
{
    public class AdminKetQuaHocTapViewModel
    {
        public int KetQuaHocTapId { get; set; }
        public string HoTenHocVien { get; set; } = string.Empty;
        public string TenHoSo { get; set; } = string.Empty;

        public int SoBuoiHoc { get; set; }
        public int SoBuoiToiThieu { get; set; }

        public decimal SoKmHoanThanh { get; set; }
        public decimal KmToiThieu { get; set; }

        public string? NhanXet { get; set; }

        public bool DuDieuKienThiTotNghiep { get; set; }
        public bool DauTotNghiep { get; set; }
        public bool DuDieuKienThiSatHach { get; set; }

        public bool LyThuyetKq { get; set; }
        public bool SaHinhKq { get; set; }
        public bool DuongTruongKq { get; set; }
        public bool MoPhongKq { get; set; }

        public bool DaHoanThanhSatHach =>
            DuDieuKienThiTotNghiep
            && DauTotNghiep
            && DuDieuKienThiSatHach
            && LyThuyetKq
            && SaHinhKq
            && DuongTruongKq
            && MoPhongKq;

        public string TrangThaiTongQuat
        {
            get
            {
                if (DaHoanThanhSatHach)
                    return "Chờ cấp bằng";

                if (DauTotNghiep && DuDieuKienThiSatHach)
                    return "Đang thi sát hạch";

                if (DuDieuKienThiTotNghiep)
                    return "Đang thi tốt nghiệp";

                return "Đang học";
            }
        }
    }
}
