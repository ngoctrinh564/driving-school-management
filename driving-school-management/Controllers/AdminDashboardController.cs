using driving_school_management.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly AdminDashboardService _service;

        public AdminDashboardController(AdminDashboardService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            DashboardVM vm = await _service.GetDashboard();
            return View(vm);
        }
    }
}