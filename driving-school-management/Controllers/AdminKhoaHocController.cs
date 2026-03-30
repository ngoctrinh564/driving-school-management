using driving_school_management.Services;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    public class AdminKhoaHocController : Controller
    {
        private readonly AdminKhoaHocService _service;

        public AdminKhoaHocController(AdminKhoaHocService service)
        {
            _service = service;
        }

        public IActionResult Index(string keyword, int? hangId, string trangThai, int page = 1)
        {
            var result = _service.GetList(keyword, hangId, trangThai, page);

            ViewBag.Total = result.Item2;
            ViewBag.Page = page;

            return View(result.Item1);
        }
    }
}
