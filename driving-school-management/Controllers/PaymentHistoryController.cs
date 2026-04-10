using driving_school_management.Services;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    public class PaymentHistoryController : Controller
    {
        private readonly PaymentHistoryService _paymentHistoryService;

        public PaymentHistoryController(PaymentHistoryService paymentHistoryService)
        {
            _paymentHistoryService = paymentHistoryService;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            return View();
        }

        [HttpGet]
        public IActionResult GetPaymentHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            try
            {
                var data = _paymentHistoryService.GetPaymentHistoryByUser(userId.Value);
                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetPaymentHistoryDetail(int phieuId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            try
            {
                var data = _paymentHistoryService.GetPaymentHistoryDetail(userId.Value, phieuId);
                if (data == null)
                    return NotFound();

                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}