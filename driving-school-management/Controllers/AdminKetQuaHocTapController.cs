using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using driving_school_management.Services;

namespace driving_school_management.Controllers
{
    public class AdminKetQuaHocTapController : Controller
    {
        private readonly IAdminKetQuaHocTapService _adminKetQuaHocTapService;

        public AdminKetQuaHocTapController(IAdminKetQuaHocTapService adminKetQuaHocTapService)
        {
            _adminKetQuaHocTapService = adminKetQuaHocTapService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _adminKetQuaHocTapService.GetAllAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatKetQuaHocTap(AdminKetQuaHocTapRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _adminKetQuaHocTapService.UpdateKetQuaHocTapAsync(request);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatChiTietKetQuaHocTap(AdminChiTietKetQuaHocTapRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _adminKetQuaHocTapService.UpdateChiTietKetQuaHocTapAsync(request);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}