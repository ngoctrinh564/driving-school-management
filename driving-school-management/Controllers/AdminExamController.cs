using Microsoft.AspNetCore.Mvc;
using driving_school_management.Services;
using driving_school_management.Models.DTOs;

namespace driving_school_management.Controllers
{
    public class AdminExamController : Controller
    {
        private readonly AdminExamService _service;

        public AdminExamController(AdminExamService service)
        {
            _service = service;
        }

        // ===================== LIST =====================
        public async Task<IActionResult> Index()
        {
            var data = await _service.GetKyThiAsync();
            return View(data);
        }

        // ===================== CREATE =====================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string tenKyThi, string loaiKyThi)
        {
            await _service.CreateKyThi(tenKyThi, loaiKyThi);
            TempData["Success"] = "Tạo kỳ thi thành công";
            return RedirectToAction("Index");
        }

        // ===================== EDIT =====================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var list = await _service.GetKyThiAsync();
            var item = list.FirstOrDefault(x => x.KyThiId == id);

            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int kyThiId, string tenKyThi, string loaiKyThi)
        {
            await _service.UpdateKyThi(kyThiId, tenKyThi, loaiKyThi);
            TempData["Success"] = "Kỳ thì được cập nhật thành công";
            return RedirectToAction("Index");
        }

        // ===================== DELETE =====================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteKyThi(id);
            return RedirectToAction("Index");
        }

        // ===================== DETAILS =====================
        public async Task<IActionResult> Details(int id)
        {
            var lichThi = await _service.GetLichThiByKyThi(id);

            ViewBag.KyThiId = id;
            return View(lichThi);
        }

        // ===================== CREATE LICHTHI =====================
        [HttpPost]
        public async Task<IActionResult> CreateLichThi(int kyThiId, DateTime thoiGianThi, string diaDiem)
        {
            await _service.CreateLichThi(kyThiId, thoiGianThi, diaDiem);
            TempData["Success"] = "Tạo lịch thi thành công";
            return RedirectToAction("Details", new { id = kyThiId });
        }
    }
}