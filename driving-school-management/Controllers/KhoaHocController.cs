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

        // Trang giao diện
        public IActionResult Index()
        {
            return View();
        }

        // API lấy danh sách khóa học đang mở
        [HttpGet]
        public IActionResult GetDanhSachKhoaHocDangMo()
        {
            var data = _service.GetKhoaHocDangMo();
            return Json(data);
        }

        // API lấy chi tiết khóa học
        [HttpGet]
        public IActionResult GetKhoaHocDetail(int id)
        {
            var data = _service.GetKhoaHocDetail(id);

            if (data == null)
                return NotFound();

            return Json(data);
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

            if (check.DaTungHocHang == 1)
            {
                return Json(new
                {
                    success = true,
                    needConfirmAgain = true,
                    message = $"Bạn đã từng học khóa học của hạng {check.TenHang} rồi. Nếu vẫn muốn học tiếp, hãy bấm tiếp tục.",
                    redirectUrl = Url.Action("Confirm", "KhoaHoc", new { khoaHocId })
                });
            }

            return Json(new
            {
                success = true,
                needConfirmAgain = false,
                redirectUrl = Url.Action("Confirm", "KhoaHoc", new { khoaHocId })
            });
        }

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

            return View(model);
        }

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

            if (model.IsMoDangKy == 0 || model.HasHoSoPhuHop == 0 || model.HasHoSoChuaDuyet == 1)
            {
                TempData["Error"] = "Không đủ điều kiện đăng ký khóa học.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            return RedirectToAction("Index", "ThanhToan", new
            {
                khoaHocId = model.KhoaHocId,
                hoSoId = model.HoSoIdPhuHop
            });
        }
    }
}
