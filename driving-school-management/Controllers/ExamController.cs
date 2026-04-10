using driving_school_management.Models.DTOs;
using driving_school_management.Services;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    public class ExamController : Controller
    {
        private readonly ExamService _service;

        public ExamController(ExamService service)
        {
            _service = service;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Auth");

            try
            {
                var data = await _service.GetKyThiForUserAsync(userId.Value);
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return View(new List<UserExamDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmRegister(int kyThiId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Auth");

            try
            {
                var model = await _service.GetConfirmDangKyInfoAsync(userId.Value, kyThiId);
                if (model == null)
                    return NotFound();

                ViewBag.SelectedKyThiId = kyThiId;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(int kyThiId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Auth");

            try
            {
                return RedirectToAction("Start", "ExamPayment", new { kyThiId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return RedirectToAction(nameof(ConfirmRegister), new { kyThiId });
            }
        }
    }
}