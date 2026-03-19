using Microsoft.AspNetCore.Mvc;

public class AdminDashboardController : Controller
{
    private readonly AdminDashboardService _service;

    public AdminDashboardController(AdminDashboardService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var vm = await _service.GetDashboard();
        return View(vm);
    }
}