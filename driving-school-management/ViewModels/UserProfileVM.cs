using System;

namespace driving_school_management.ViewModels
{
    public class UserProfileVM
    {
        public int UserId { get; set; }
        public int HocVienId { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }

        public string HoTen { get; set; }
        public string SoCmndCccd { get; set; }
        public DateTime? NamSinh { get; set; }
        public string GioiTinh { get; set; }
        public string Sdt { get; set; }
        public string AvatarUrl { get; set; }

        public bool IsMissingHoTen => string.IsNullOrWhiteSpace(HoTen);
        public bool IsMissingSoCmndCccd => string.IsNullOrWhiteSpace(SoCmndCccd);
        public bool IsMissingNamSinh => !NamSinh.HasValue;
        public bool IsMissingGioiTinh => string.IsNullOrWhiteSpace(GioiTinh);
        public bool IsMissingSdt => string.IsNullOrWhiteSpace(Sdt);

        public bool IsProfileIncomplete =>
            IsMissingHoTen ||
            IsMissingSoCmndCccd ||
            IsMissingNamSinh ||
            IsMissingGioiTinh ||
            IsMissingSdt;
    }
}