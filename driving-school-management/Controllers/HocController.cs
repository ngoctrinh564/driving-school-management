using driving_school_management.Models.DTOs;
using driving_school_management.Services;
using driving_school_management.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace driving_school_management.Controllers
{
    public class HocController : Controller
    {
        private readonly HocService _hocService;

        public HocController(HocService hocService)
        {
            _hocService = hocService;
        }

        public IActionResult Index(bool open = false)
        {
            var vm = new HocDashboardViewModel();

            var hangTable = _hocService.GetHangList();
            vm.ListHang = hangTable.AsEnumerable()
                .Select(r => r["MAHANG"]?.ToString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            vm.SelectedHang = HttpContext.Session.GetString("Hang");
            int? userId = HttpContext.Session.GetInt32("UserId");

            vm.ShowPopup = open || string.IsNullOrWhiteSpace(vm.SelectedHang);

            if (string.IsNullOrWhiteSpace(vm.SelectedHang))
                return View(vm);

            string hangDaChon = vm.SelectedHang.Trim().ToUpper();
            vm.SelectedHang = hangDaChon;

            var dt = _hocService.GetHocDashboard(hangDaChon, userId);
            if (dt.Rows.Count == 0)
            {
                vm.ShowPopup = true;
                return View(vm);
            }

            var row = dt.Rows[0];

            vm.ThoiGianThi = int.Parse(row["THOIGIANTHI"].ToString() ?? "0");
            vm.SoCauThiNgauNhien = int.Parse(row["SOCAUTHINGAUNHIEN"].ToString() ?? "0");
            vm.TotalBoDe = int.Parse(row["TOTAL_BODE"].ToString() ?? "0");
            vm.DoneBoDe = int.Parse(row["DONE_BODE"].ToString() ?? "0");
            vm.TotalCauHoi = int.Parse(row["TOTAL_CAUHOI"].ToString() ?? "0");
            vm.TotalCauLiet = int.Parse(row["TOTAL_CAULIET"].ToString() ?? "0");
            vm.TotalCauChuY = int.Parse(row["TOTAL_CAUCHUY"].ToString() ?? "0");
            vm.TotalBienBao = int.Parse(row["TOTAL_BIENBAO"].ToString() ?? "0");

            vm.HasMoPhong = int.Parse(row["HAS_MOPHONG"].ToString() ?? "0") == 1;
            if (vm.HasMoPhong)
            {
                vm.MpBoDe = int.Parse(row["MP_BODE"].ToString() ?? "0");
                vm.MpTinhHuong = int.Parse(row["MP_TINHHUONG"].ToString() ?? "0");
                vm.MpBoDeDone = int.Parse(row["MP_BODE_DONE"].ToString() ?? "0");
            }

            return View(vm);
        }

        public IActionResult CauLiet()
        {
            string hang = HttpContext.Session.GetString("Hang")?.Trim().ToUpper() ?? "";
            bool isXeMay = hang == "A" || hang == "A1";

            var dt = _hocService.GetCauLiet(hang);

            var chapters = dt.AsEnumerable()
                .GroupBy(r => new
                {
                    ChuongId = _hocService.GetNumberAsInt(r["CHUONGID"]),
                    TenChuong = r["TENCHUONG"]?.ToString() ?? "",
                    ThuTu = _hocService.GetNumberAsInt(r["CHUONG_THUTU"])
                })
                .Select(g => new HocAllChapterVM
                {
                    ChuongId = g.Key.ChuongId,
                    TenChuong = g.Key.TenChuong,
                    ThuTu = g.Key.ThuTu,
                    Questions = g.GroupBy(r => new
                    {
                        GlobalIndex = _hocService.GetNumberAsInt(r["GLOBAL_INDEX"]),
                        IdCauHoi = _hocService.GetNumberAsInt(r["CAUHOIID"]),
                        NoiDung = r["NOIDUNG"]?.ToString() ?? "",
                        HinhAnh = r["HINHANH"]?.ToString(),
                        UrlAnhMeo = r["URLANHMEO"]?.ToString(),
                        IsCauLiet = _hocService.GetNumberAsInt(r["CAULIET"]) == 1,
                        IsChuY = _hocService.GetNumberAsInt(r["CHUY"]) == 1,
                        IsXeMay = _hocService.GetNumberAsInt(r["XEMAY"]) == 1
                    })
                    .Select(q => new HocAllQuestionVM
                    {
                        GlobalIndex = q.Key.GlobalIndex,
                        IdCauHoi = q.Key.IdCauHoi,
                        NoiDung = q.Key.NoiDung,
                        ImageUrl = NormalizeImage(q.Key.HinhAnh),
                        UrlAnhMeo = NormalizeImage(q.Key.UrlAnhMeo),
                        IsCauLiet = q.Key.IsCauLiet,
                        IsChuY = q.Key.IsChuY,
                        IsXeMay = q.Key.IsXeMay,
                        DapAns = q.OrderBy(x => _hocService.GetNumberAsInt(x["DAPAN_THUTU"]))
                            .Select((x, idx) => new HocAllAnswerVM
                            {
                                IdDapAn = _hocService.GetNumberAsInt(x["DAPANID"]),
                                Label = (idx + 1).ToString(),
                                IsCorrect = _hocService.GetNumberAsInt(x["DAPANDUNG"]) == 1
                            })
                            .ToList()
                    })
                    .OrderBy(x => x.GlobalIndex)
                    .ToList()
                })
                .OrderBy(c => c.ThuTu)
                .ToList();

            var vm = new HocAllViewModel
            {
                SelectedHang = hang,
                IsXeMay = isXeMay,
                Chapters = chapters,
                TotalQuestions = chapters.Sum(c => c.Questions.Count),
                TotalChapters = chapters.Count
            };

            return View(vm);
        }

        public IActionResult ChuY()
        {
            string hang = HttpContext.Session.GetString("Hang")?.Trim().ToUpper() ?? "";
            bool isXeMay = hang == "A" || hang == "A1";

            var dt = _hocService.GetChuY(hang);

            var chapters = dt.AsEnumerable()
                .GroupBy(r => new
                {
                    ChuongId = _hocService.GetNumberAsInt(r["CHUONGID"]),
                    TenChuong = r["TENCHUONG"]?.ToString() ?? "",
                    ThuTu = _hocService.GetNumberAsInt(r["CHUONG_THUTU"])
                })
                .Select(g => new HocAllChapterVM
                {
                    ChuongId = g.Key.ChuongId,
                    TenChuong = g.Key.TenChuong,
                    ThuTu = g.Key.ThuTu,
                    Questions = g.GroupBy(r => new
                    {
                        GlobalIndex = _hocService.GetNumberAsInt(r["GLOBAL_INDEX"]),
                        IdCauHoi = _hocService.GetNumberAsInt(r["CAUHOIID"]),
                        NoiDung = r["NOIDUNG"]?.ToString() ?? "",
                        HinhAnh = r["HINHANH"]?.ToString(),
                        UrlAnhMeo = r["URLANHMEO"]?.ToString(),
                        IsCauLiet = _hocService.GetNumberAsInt(r["CAULIET"]) == 1,
                        IsChuY = _hocService.GetNumberAsInt(r["CHUY"]) == 1,
                        IsXeMay = _hocService.GetNumberAsInt(r["XEMAY"]) == 1
                    })
                    .Select(q => new HocAllQuestionVM
                    {
                        GlobalIndex = q.Key.GlobalIndex,
                        IdCauHoi = q.Key.IdCauHoi,
                        NoiDung = q.Key.NoiDung,
                        ImageUrl = NormalizeImage(q.Key.HinhAnh),
                        UrlAnhMeo = NormalizeImage(q.Key.UrlAnhMeo),
                        IsCauLiet = q.Key.IsCauLiet,
                        IsChuY = q.Key.IsChuY,
                        IsXeMay = q.Key.IsXeMay,
                        DapAns = q.OrderBy(x => _hocService.GetNumberAsInt(x["DAPAN_THUTU"]))
                            .Select((x, idx) => new HocAllAnswerVM
                            {
                                IdDapAn = _hocService.GetNumberAsInt(x["DAPANID"]),
                                Label = (idx + 1).ToString(),
                                IsCorrect = _hocService.GetNumberAsInt(x["DAPANDUNG"]) == 1
                            })
                            .ToList()
                    })
                    .OrderBy(x => x.GlobalIndex)
                    .ToList()
                })
                .OrderBy(c => c.ThuTu)
                .ToList();

            var vm = new HocAllViewModel
            {
                SelectedHang = hang,
                IsXeMay = isXeMay,
                Chapters = chapters,
                TotalQuestions = chapters.Sum(c => c.Questions.Count),
                TotalChapters = chapters.Count
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult ChonHang(string maHang)
        {
            if (!string.IsNullOrWhiteSpace(maHang))
                HttpContext.Session.SetString("Hang", maHang.Trim().ToUpper());

            return RedirectToAction("Index");
        }

        public IActionResult FlashCardBienBao()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            bool isLoggedIn = userId.HasValue && userId.Value > 0;

            var dt = _hocService.GetFlashCardBienBao(userId);

            var cards = dt.AsEnumerable()
                .Select(r => new BienBaoFlashCardVM
                {
                    IdBienBao = _hocService.GetNumberAsInt(r["IDBIENBAO"]),
                    TenBienBao = r["TENBIENBAO"]?.ToString() ?? "",
                    YNghia = r["YNGHIA"]?.ToString(),
                    HinhAnh = NormalizeImage(r["HINHANH"]?.ToString()),
                    IdFlashcard = r["IDFLASHCARD"] == DBNull.Value ? null : _hocService.GetNumberAsInt(r["IDFLASHCARD"]),
                    DanhGia = r["DANHGIA"]?.ToString()
                })
                .ToList();

            var vm = new BienBaoFlashStudyPageVM
            {
                IsLoggedIn = isLoggedIn,
                LoginUrl = "/Auth/Login",
                Cards = cards
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Save([FromBody] SaveFlashcardDto dto)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || userId.Value <= 0)
                return Unauthorized(new { message = "Bạn cần đăng nhập để lưu tiến trình." });

            _hocService.SaveFlashCard(userId.Value, dto.IdBienBao, dto.DanhGia);

            return Ok(new
            {
                ok = true,
                idBienBao = dto.IdBienBao,
                danhGia = dto.DanhGia
            });
        }

        private string? NormalizeImage(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            fileName = fileName.Trim();

            if (fileName.StartsWith("wwwwroot"))
                fileName = fileName.Replace("wwwwroot", "").TrimStart('/');

            if (fileName.StartsWith("wwwroot"))
                fileName = fileName.Replace("wwwroot", "").TrimStart('/');

            if (fileName.StartsWith("~/"))
                return fileName.Replace("~/", "/");

            if (fileName.StartsWith("images"))
                return "/" + fileName;

            if (fileName.StartsWith("/images"))
                return fileName;

            return "/images/cau_hoi/" + fileName;
        }
    }
}