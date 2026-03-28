using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    public class KhoaHocController : Controller
    {
        private readonly KhoaHocService _service;

        public KhoaHocController(KhoaHocService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetDanhSachKhoaHocDangMo()
        {
            var data = _service.GetKhoaHocDangMo();
            return Json(data);
        }

        [HttpGet]
        public IActionResult GetKhoaHocDetail(int id)
        {
            var data = _service.GetKhoaHocDetail(id);

            if (data == null)
                return NotFound();

            return Json(data);
        }

        // ✅ thêm mới
        [HttpGet]
        public IActionResult GetHoSoStatusIndex()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new
                {
                    isLoggedIn = false
                });
            }

            var data = _service.GetHoSoStatusIndex(userId.Value);
            if (data == null)
            {
                return Json(new
                {
                    isLoggedIn = true,
                    showModal = false
                });
            }

            return Json(new
            {
                isLoggedIn = true,
                showModal = data.ShowModal == 1,
                statusCode = data.StatusCode,
                message = data.StatusMessage,
                redirectUrl = Url.Action("Create", "HoSo")
            });
        }

        [HttpGet]
        public IActionResult CheckDangKyKhoaHoc(int khoaHocId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new
                {
                    success = false,
                    requireLogin = true,
                    message = "Vui lòng đăng nhập để đăng ký khóa học."
                });
            }

            var check = _service.CheckDangKyKhoaHoc(userId.Value, khoaHocId);
            if (check == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy khóa học."
                });
            }

            // ❗ bổ sung kiểm tra mới (ưu tiên statusCode từ DB)
            if (check.CoTheDangKy == 0)
            {
                return Json(new
                {
                    success = false,
                    statusCode = check.StatusCode,
                    needProfile = check.StatusCode == "NO_PROFILE"
                               || check.StatusCode == "ALL_PROFILE_EXPIRED"
                               || check.StatusCode == "NO_PROFILE_FOR_HANG"
                               || check.StatusCode == "PROFILE_EXPIRED"
                               || check.StatusCode == "PROFILE_REJECTED",
                    needApprovedProfile = check.StatusCode == "PENDING_APPROVAL",
                    message = check.StatusMessage,
                    redirectUrl = (check.StatusCode == "NO_PROFILE"
                                || check.StatusCode == "ALL_PROFILE_EXPIRED"
                                || check.StatusCode == "NO_PROFILE_FOR_HANG"
                                || check.StatusCode == "PROFILE_EXPIRED"
                                || check.StatusCode == "PROFILE_REJECTED")
                                ? Url.Action("Create", "HoSo")
                                : null
                });
            }

            // ================= GIỮ NGUYÊN CODE CŨ =================

            if (check.IsMoDangKy == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Khóa học này hiện không mở đăng ký."
                });
            }

            if (check.HasHoSoPhuHop == 0)
            {
                if (check.HasHoSoChuaDuyet == 1)
                {
                    return Json(new
                    {
                        success = false,
                        needProfile = false,
                        needApprovedProfile = true,
                        message = $"Bạn đã có hồ sơ hạng {check.TenHang} nhưng hồ sơ chưa được duyệt, nên hiện chưa thể đăng ký khóa học."
                    });
                }

                return Json(new
                {
                    success = false,
                    needProfile = true,
                    needApprovedProfile = false,
                    message = $"Bạn chưa có hồ sơ phù hợp với hạng {check.TenHang}. Vui lòng thêm hồ sơ trước khi đăng ký."
                });
            }

            if (check.BiTrungThoiGianHoc == 1)
            {
                var tuNgay = check.NgayBatDauTrungThoiGian?.ToString("dd/MM/yyyy") ?? "";
                var denNgay = check.NgayKetThucTrungThoiGian?.ToString("dd/MM/yyyy") ?? "";

                return Json(new
                {
                    success = false,
                    blockedByTimeConflict = true,
                    message = $"Bạn đã đăng ký khóa học \"{check.TenKhoaHocTrungThoiGian}\" có thời gian học từ {tuNgay} đến {denNgay}. Bạn không thể đăng ký thêm khóa học khác trong khoảng thời gian này."
                });
            }

            if (check.DaTungDangKyCungHang == 1)
            {
                return Json(new
                {
                    success = true,
                    needConfirmAgain = true,
                    message = $"Bạn đã từng đăng ký khóa học hạng {check.TenHang}. Nếu vẫn muốn tiếp tục đăng ký khóa học \"{check.TenKhoaHoc}\" thì hãy xác nhận để tiếp tục.",
                    redirectUrl = Url.Action("Confirm", "KhoaHoc", new { khoaHocId })
                });
            }

            if (check.DaDangKyChinhKhoaHoc == 1)
            {
                return Json(new
                {
                    success = false,
                    alreadyRegisteredSameCourse = true,
                    message = $"Bạn đã đăng ký khóa học \"{check.TenKhoaHocDaDangKy}\" rồi, không thể đăng ký lại."
                });
            }

            return Json(new
            {
                success = true,
                needConfirmAgain = false,
                redirectUrl = Url.Action("Confirm", "KhoaHoc", new { khoaHocId })
            });
        }

        // ❗ GIỮ NGUYÊN 100%
        [HttpGet]
        public IActionResult Confirm(int khoaHocId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var model = _service.GetKhoaHocConfirmInfo(userId.Value, khoaHocId);
            if (model == null)
                return NotFound();

            if (model.IsMoDangKy == 0)
            {
                TempData["Error"] = "Khóa học này hiện không mở đăng ký.";
                return RedirectToAction("Index");
            }

            if (model.HasHoSoPhuHop == 0)
            {
                if (model.HasHoSoChuaDuyet == 1)
                {
                    TempData["Error"] = $"Bạn đã có hồ sơ hạng {model.TenHang} nhưng hồ sơ chưa được duyệt.";
                    return RedirectToAction("Index", "KhoaHoc");
                }

                TempData["Error"] = $"Bạn chưa có hồ sơ phù hợp với hạng {model.TenHang}.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            if (model.BiTrungThoiGianHoc == 1)
            {
                var tuNgay = model.NgayBatDauTrungThoiGian?.ToString("dd/MM/yyyy") ?? "";
                var denNgay = model.NgayKetThucTrungThoiGian?.ToString("dd/MM/yyyy") ?? "";

                TempData["Error"] = $"Bạn đã có khóa học trùng thời gian học từ {tuNgay} đến {denNgay}, nên không thể đăng ký thêm.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            if (model.DaDangKyChinhKhoaHoc == 1)
            {
                TempData["Error"] = $"Bạn đã đăng ký khóa học \"{model.TenKhoaHocDaDangKy}\" rồi.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            return View(model);
        }

        // ❗ GIỮ NGUYÊN 100%
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XacNhanDangKy(int khoaHocId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var model = _service.GetKhoaHocConfirmInfo(userId.Value, khoaHocId);
            if (model == null)
                return NotFound();

            if (model.IsMoDangKy == 0
                || model.HasHoSoPhuHop == 0
                || model.HasHoSoChuaDuyet == 1
                || model.DaDangKyChinhKhoaHoc == 1
                || model.BiTrungThoiGianHoc == 1
                || model.CoTheDangKy == 0)
            {
                TempData["Error"] = "Không đủ điều kiện đăng ký khóa học.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            return RedirectToAction("StartPayment", "Payment", new
            {
                khoaHocId = model.KhoaHocId,
                hoSoId = model.HoSoIdPhuHop
            });
        }
    }
}