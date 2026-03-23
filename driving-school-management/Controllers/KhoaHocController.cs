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
    }
}
