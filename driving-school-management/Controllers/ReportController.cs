using driving_school_management.Services;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    [Route("Report")]
    public class ReportController : Controller
    {
        private readonly ReportOracleService _service;

        public ReportController(ReportOracleService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var model = await _service.GetDashboardAsync(fromDate, toDate);
            return View(model); // không cần ghi full path
        }

        [HttpGet("Tab/{tab}")]
        public async Task<IActionResult> Tab(string tab, DateTime? fromDate, DateTime? toDate)
        {
            var model = await _service.GetDashboardAsync(fromDate, toDate);

            return tab switch
            {
                "users" => PartialView("~/Views/Report/_TabUsers.cshtml", model),
                "courses" => PartialView("~/Views/Report/_TabCourses.cshtml", model),
                "exams" => PartialView("~/Views/Report/_TabExams.cshtml", model),
                _ => PartialView("~/Views/Report/_TabOverview.cshtml", model)
            };
        }

        [HttpGet("ExportPdf")]
        public async Task<IActionResult> ExportPdf(DateTime? fromDate, DateTime? toDate, string? tab)
        {
            var model = await _service.GetDashboardAsync(fromDate, toDate);
            var pdf = _service.GenerateDashboardPdf(model, fromDate, toDate, tab);

            return File(
                pdf,
                "application/pdf",
                $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}