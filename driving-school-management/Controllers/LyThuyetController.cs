using driving_school_management.Services;
using driving_school_management.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace driving_school_management.Controllers
{
    public class LyThuyetController : Controller
    {
        private readonly LyThuyetService _service;

        public LyThuyetController(LyThuyetService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var selectedHang = HttpContext.Session.GetString("Hang");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrWhiteSpace(selectedHang))
                return RedirectToAction("Index", "Hoc");

            var dsBoDe = _service.GetBoDe(selectedHang);

            if (userId.HasValue)
                ViewBag.BaiLamDict = BuildBaiLamDict(_service.GetLastBaiLamByHang(userId.Value, selectedHang));
            else
                ViewBag.BaiLamDict = new Dictionary<int, BaiLamHistoryItemVM>();

            return View(dsBoDe);
        }

        [HttpGet]
        public IActionResult Exam(int idBoDe, bool history = false)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (history && userId.HasValue)
            {
                var examDt = _service.GetExam(idBoDe);
                var historyDt = _service.GetHistory(userId.Value, idBoDe);

                if (examDt.Rows.Count > 0 && historyDt.Rows.Count > 0)
                {
                    var vmHistory = BuildHistoryViewModel(examDt, historyDt);
                    return View(vmHistory);
                }
            }

            var examVm = BuildExamViewModel(idBoDe);
            if (examVm == null)
            {
                TempData["Error"] = "Bộ đề không tồn tại hoặc không khả dụng.";
                return RedirectToAction("Index");
            }

            return View(examVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Exam(int idBoDe, int timeLeftSeconds)
        {
            ExamViewModel? vm;

            if (idBoDe == -1)
            {
                vm = BuildRandomExamViewModel_FromSession();
                if (vm == null)
                {
                    TempData["Error"] = "Phiên thi ngẫu nhiên đã hết hạn.";
                    return RedirectToAction("RandomExam");
                }
            }
            else
            {
                vm = BuildExamViewModel(idBoDe);
            }

            if (vm == null)
            {
                TempData["Error"] = "Bộ đề không tồn tại hoặc không khả dụng.";
                return RedirectToAction("Index");
            }

            int totalSeconds = vm.ThoiGian * 60;
            if (totalSeconds <= 0) totalSeconds = 20 * 60;

            int usedSeconds = totalSeconds - timeLeftSeconds;
            if (usedSeconds < 0) usedSeconds = 0;
            if (usedSeconds > totalSeconds) usedSeconds = totalSeconds;

            vm.ThoiGianLam = usedSeconds;
            vm.IsSubmitted = true;

            foreach (var q in vm.CauHoi)
            {
                string key = $"answer_{q.IdCauHoi}";
                var value = Request.Form[key];

                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var ansId))
                    vm.DapAnDaChon[q.IdCauHoi] = ansId;
                else
                    vm.DapAnDaChon[q.IdCauHoi] = null;
            }

            int correct = 0;
            bool cauLietSai = false;

            foreach (var q in vm.CauHoi)
            {
                var correctAnswer = q.DapAn.FirstOrDefault(a => a.IsCorrect);
                vm.DapAnDaChon.TryGetValue(q.IdCauHoi, out int? userAnsId);

                bool isCorrect = correctAnswer != null &&
                                 userAnsId.HasValue &&
                                 userAnsId.Value == correctAnswer.IdDapAn;

                if (isCorrect)
                {
                    correct++;
                }
                else if (userAnsId.HasValue)
                {
                    if (q.LaCauLiet)
                        cauLietSai = true;
                }
            }

            vm.SoCauDung = correct;
            vm.SoCauSai = vm.TongCau - correct;
            vm.CoCauLietSai = cauLietSai;
            vm.Dat = (correct >= vm.DiemDat) && !cauLietSai;

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue && !vm.IsRandomExam)
            {
                SaveExamResultToDatabase(userId.Value, vm);
            }

            return View(vm);
        }

        public IActionResult RandomExam()
        {
            var hang = HttpContext.Session.GetString("Hang");
            if (string.IsNullOrWhiteSpace(hang))
                return RedirectToAction("Index", "Hoc");

            var dt = _service.GetRandomExam(hang);
            if (dt.Rows.Count == 0)
            {
                TempData["Error"] = "Không tạo được đề ngẫu nhiên.";
                return RedirectToAction("Index");
            }

            var listIds = dt.AsEnumerable()
                .Select(r => Convert.ToInt32(r["CAUHOIID"]))
                .Distinct()
                .ToList();

            HttpContext.Session.SetString("RandomExamCauHoi", string.Join(",", listIds));

            var vm = BuildRandomExamViewModel(dt, hang);
            vm.ThoiGian = HttpContext.Session.GetInt32("RandomExamTime") ?? vm.ThoiGian;
            vm.IsRandomExam = true;

            return View("Exam", vm);
        }

        public IActionResult HocAll()
        {
            var vm = new HocAllViewModel();

            var selectedHang = HttpContext.Session.GetString("Hang");
            if (string.IsNullOrWhiteSpace(selectedHang))
                return RedirectToAction("Index", "Hoc", new { open = true });

            selectedHang = selectedHang.Trim().ToUpper();
            vm.SelectedHang = selectedHang;
            vm.IsXeMay = selectedHang == "A" || selectedHang == "A1";

            var dt = _service.GetHocAll(selectedHang);
            if (dt.Rows.Count == 0)
                return View(vm);

            var chapterMap = new Dictionary<int, HocAllChapterVM>();

            foreach (DataRow row in dt.Rows)
            {
                int chuongId = Convert.ToInt32(row["CHUONGID"]);
                int cauHoiId = Convert.ToInt32(row["CAUHOIID"]);

                if (!chapterMap.ContainsKey(chuongId))
                {
                    chapterMap[chuongId] = new HocAllChapterVM
                    {
                        ChuongId = chuongId,
                        TenChuong = row["TENCHUONG"]?.ToString() ?? string.Empty,
                        ThuTu = Convert.ToInt32(row["CHUONGTHUTU"])
                    };
                }

                var chapter = chapterMap[chuongId];

                var question = chapter.Questions.FirstOrDefault(x => x.IdCauHoi == cauHoiId);
                if (question == null)
                {
                    question = new HocAllQuestionVM
                    {
                        GlobalIndex = Convert.ToInt32(row["GLOBAL_INDEX"]),
                        IdCauHoi = cauHoiId,
                        NoiDung = row["NOIDUNG"]?.ToString() ?? string.Empty,
                        ImageUrl = NormalizeImagePath(row["HINHANH"]?.ToString()),
                        UrlAnhMeo = NormalizeImagePath(row["URLANHMEO"]?.ToString()),
                        IsCauLiet = Convert.ToInt32(row["CAULIET"]) == 1,
                        IsChuY = Convert.ToInt32(row["CHUY"]) == 1,
                        IsXeMay = Convert.ToInt32(row["XEMAY"]) == 1
                    };

                    chapter.Questions.Add(question);
                }

                question.DapAns.Add(new HocAllAnswerVM
                {
                    IdDapAn = Convert.ToInt32(row["DAPANID"]),
                    Label = Convert.ToInt32(row["DAPANTHUTU"]).ToString(),
                    IsCorrect = Convert.ToInt32(row["DAPANDUNG"]) == 1
                });
            }

            vm.Chapters = chapterMap.Values
                .OrderBy(x => x.ThuTu)
                .ThenBy(x => x.ChuongId)
                .ToList();

            foreach (var chapter in vm.Chapters)
            {
                chapter.Questions = chapter.Questions
                    .OrderBy(x => x.GlobalIndex)
                    .ToList();

                foreach (var question in chapter.Questions)
                {
                    question.DapAns = question.DapAns
                        .OrderBy(x => int.Parse(x.Label))
                        .ToList();
                }
            }

            vm.TotalChapters = vm.Chapters.Count;
            vm.TotalQuestions = vm.Chapters.Sum(x => x.Questions.Count);

            return View(vm);
        }

        private ExamViewModel BuildHistoryViewModel(DataTable examDt, DataTable historyDt)
        {
            var vm = BuildExamViewModelFromTable(examDt, false);

            if (vm == null)
                return new ExamViewModel();

            vm.IsSubmitted = true;

            var firstHistory = historyDt.Rows[0];
            vm.ThoiGianLam = Convert.ToInt32(firstHistory["THOIGIANLAMBAI"]);
            vm.SoCauSai = Convert.ToInt32(firstHistory["SOCAUSAI"]);
            vm.SoCauDung = vm.TongCau - vm.SoCauSai;
            vm.Dat = Convert.ToInt32(firstHistory["KETQUA"]) == 1;

            foreach (DataRow row in historyDt.Rows)
            {
                int cauHoiId = Convert.ToInt32(row["CAUHOIID"]);
                var raw = row["DAPANDACHON"]?.ToString();

                if (!string.IsNullOrWhiteSpace(raw) && int.TryParse(raw, out int ans))
                    vm.DapAnDaChon[cauHoiId] = ans;
                else
                    vm.DapAnDaChon[cauHoiId] = null;
            }

            vm.CoCauLietSai = vm.CauHoi.Any(q =>
            {
                if (!q.LaCauLiet) return false;
                vm.DapAnDaChon.TryGetValue(q.IdCauHoi, out int? userAns);
                var correct = q.DapAn.FirstOrDefault(a => a.IsCorrect);
                return userAns.HasValue && correct != null && userAns.Value != correct.IdDapAn;
            });

            return vm;
        }

        private ExamViewModel? BuildExamViewModel(int idBoDe)
        {
            var selectedHang = HttpContext.Session.GetString("Hang");
            if (string.IsNullOrWhiteSpace(selectedHang))
                return null;

            var dt = _service.GetExam(idBoDe);
            if (dt.Rows.Count == 0)
                return null;

            var vm = BuildExamViewModelFromTable(dt, true);
            if (vm == null)
                return null;

            if (!string.Equals(vm.Hang?.Trim(), selectedHang.Trim(), StringComparison.OrdinalIgnoreCase))
                return null;

            return vm;
        }

        private ExamViewModel BuildRandomExamViewModel(DataTable dt, string hang)
        {
            var vm = BuildExamViewModelFromTable(dt, true) ?? new ExamViewModel();

            vm.IdBoDe = -1;
            vm.TenBoDe = "Đề thi ngẫu nhiên hạng " + hang;
            vm.Hang = hang;
            vm.IsRandomExam = true;

            return vm;
        }

        private ExamViewModel? BuildRandomExamViewModel_FromSession()
        {
            string? raw = HttpContext.Session.GetString("RandomExamCauHoi");
            string? hang = HttpContext.Session.GetString("Hang");

            if (string.IsNullOrWhiteSpace(raw) || string.IsNullOrWhiteSpace(hang))
                return null;

            var dt = _service.GetRandomExam(hang);
            if (dt.Rows.Count == 0)
                return null;

            var ids = raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(int.Parse)
                         .ToList();

            var filteredRows = dt.AsEnumerable()
                                 .Where(r => ids.Contains(Convert.ToInt32(r["CAUHOIID"])))
                                 .OrderBy(r => ids.IndexOf(Convert.ToInt32(r["CAUHOIID"])))
                                 .ThenBy(r => Convert.ToInt32(r["THUTU"]))
                                 .ToList();

            if (filteredRows.Count == 0)
                return null;

            var rows = filteredRows.CopyToDataTable();

            var vm = BuildRandomExamViewModel(rows, hang);
            vm.ThoiGian = HttpContext.Session.GetInt32("RandomExamTime") ?? vm.ThoiGian;
            vm.IsRandomExam = true;

            return vm;
        }

        private ExamViewModel? BuildExamViewModelFromTable(DataTable dt, bool shuffleAnswers)
        {
            if (dt.Rows.Count == 0)
                return null;

            var first = dt.Rows[0];

            var vm = new ExamViewModel
            {
                IdBoDe = Convert.ToInt32(first["BODEID"]),
                TenBoDe = first["TENBODE"]?.ToString() ?? string.Empty,
                Hang = first["MAHANG"]?.ToString() ?? string.Empty,
                ThoiGian = Convert.ToInt32(first["THOIGIAN"]),
                TongCau = Convert.ToInt32(first["SOCAUHOI"]),
                DiemDat = Convert.ToInt32(first["DIEMDAT"]),
                IsRandomExam = false
            };

            var grouped = dt.AsEnumerable()
                .GroupBy(r => Convert.ToInt32(r["CAUHOIID"]))
                .ToList();

            foreach (var g in grouped)
            {
                var row = g.First();

                var qVm = new ExamQuestionVM
                {
                    IdCauHoi = Convert.ToInt32(row["CAUHOIID"]),
                    NoiDung = row["NOIDUNG"]?.ToString() ?? string.Empty,
                    LaCauLiet = Convert.ToInt32(row["CAULIET"]) == 1,
                    ImageUrl = NormalizeImagePath(row["HINHANH"]?.ToString()),
                    UrlAnhMeo = NormalizeImagePath(row["URLANHMEO"]?.ToString())
                };

                var dapAns = g
                    .OrderBy(r => Convert.ToInt32(r["THUTU"]))
                    .Select((r, index) => new ExamAnswerVM
                    {
                        IdDapAn = Convert.ToInt32(r["DAPANID"]),
                        Label = (index + 1).ToString(),
                        IsCorrect = Convert.ToInt32(r["DAPANDUNG"]) == 1
                    })
                    .ToList();

                if (shuffleAnswers)
                    qVm.DapAn = dapAns.OrderBy(_ => Guid.NewGuid()).ToList();
                else
                    qVm.DapAn = dapAns;

                vm.CauHoi.Add(qVm);

                if (!string.IsNullOrWhiteSpace(qVm.UrlAnhMeo))
                    vm.DanhSachMeo.Add(qVm.UrlAnhMeo);
            }

            if (vm.TongCau <= 0)
                vm.TongCau = vm.CauHoi.Count;

            return vm;
        }

        private void SaveExamResultToDatabase(int userId, ExamViewModel vm)
        {
            int baiLamId = _service.SaveBaiLam(userId, vm.IdBoDe, vm.ThoiGianLam, vm.SoCauSai, vm.Dat);

            foreach (var q in vm.CauHoi)
            {
                vm.DapAnDaChon.TryGetValue(q.IdCauHoi, out int? ansId);

                var correctAnswer = q.DapAn.FirstOrDefault(a => a.IsCorrect);
                bool isCorrect = correctAnswer != null &&
                                 ansId.HasValue &&
                                 ansId.Value == correctAnswer.IdDapAn;

                _service.SaveChiTiet(
                    baiLamId,
                    q.IdCauHoi,
                    ansId?.ToString(),
                    isCorrect
                );
            }
        }

        private Dictionary<int, BaiLamHistoryItemVM> BuildBaiLamDict(DataTable dt)
        {
            var dict = new Dictionary<int, BaiLamHistoryItemVM>();

            foreach (DataRow row in dt.Rows)
            {
                int boDeId = Convert.ToInt32(row["BODEID"]);

                dict[boDeId] = new BaiLamHistoryItemVM
                {
                    BaiLamId = Convert.ToInt32(row["BAILAMID"]),
                    IdBoDe = boDeId,
                    ThoiGianLamBai = Convert.ToInt32(row["THOIGIANLAMBAI"]),
                    SoCauSai = Convert.ToInt32(row["SOCAUSAI"]),
                    KetQua = Convert.ToInt32(row["KETQUA"]) == 1
                };
            }

            return dict;
        }

        private string? NormalizeImagePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            path = path.Replace("\\", "/");

            if (path.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
                path = path.Substring(8);

            return "~/" + path.TrimStart('/');
        }
    }
}