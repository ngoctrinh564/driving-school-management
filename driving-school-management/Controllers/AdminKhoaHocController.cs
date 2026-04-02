using driving_school_management.Services;
using driving_school_management.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    public class AdminKhoaHocController : Controller
    {
        private readonly AdminKhoaHocService _adminKhoaHocService;

        public AdminKhoaHocController(AdminKhoaHocService adminKhoaHocService)
        {
            _adminKhoaHocService = adminKhoaHocService;
        }

        [HttpGet]
        public IActionResult Index(string? keyword, int? hangId, string? trangThai, int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }

            var result = _adminKhoaHocService.GetList(keyword, hangId, trangThai, page);

            ViewBag.Page = page;
            ViewBag.Total = result.Total;
            ViewBag.TotalPages = (int)Math.Ceiling((double)result.Total / 10);
            ViewBag.Hangs = _adminKhoaHocService.GetHangs();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CoursesTable", result.Data);
            }

            return View(result.Data);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var model = _adminKhoaHocService.GetDetail(id);
            if (model == null)
            {
                return NotFound();
            }

            return PartialView("_CourseDetailModal", model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Hangs = _adminKhoaHocService.GetHangs();
            return View(new KhoaHocFormVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(KhoaHocFormVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Hangs = _adminKhoaHocService.GetHangs();
                return View(model);
            }

            _adminKhoaHocService.Insert(model);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var model = _adminKhoaHocService.GetDetail(id);
            if (model == null)
            {
                return NotFound();
            }

            ViewBag.Hangs = _adminKhoaHocService.GetHangs();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(KhoaHocFormVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Hangs = _adminKhoaHocService.GetHangs();
                return View(model);
            }

            _adminKhoaHocService.Update(model);
            return RedirectToAction("Index");
        }
    }
}