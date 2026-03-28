using driving_school_management.Models;
using driving_school_management.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class HoSoController : Controller
{
    private readonly HoSoService _service;

    public HoSoController(HoSoService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        var data = _service.GetMyHoSoPage(userId.Value);
        return View(data);
    }

    public IActionResult Detail(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        var data = _service.GetDetailByUser(id, userId.Value);
        if (data == null)
        {
            return NotFound();
        }

        return PartialView("_Detail", data);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.HocVienName = _service.GetHocVienNameByUserId(userId.Value);

        ViewBag.HangOptions = new SelectList(_service.GetHangGplxOptions(), "HangId", "TenHang");
        ViewBag.LoaiHoSoOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "Đăng ký mới", Text = "Đăng ký mới" },
            new SelectListItem { Value = "Nâng hạng", Text = "Nâng hạng" }
        };

        return View(new CreateHoSoDto());
    }

    [HttpPost]
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
    [RequestSizeLimit(104857600)]
    public async Task<IActionResult> Create([FromForm] CreateHoSoDto? model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        model ??= new CreateHoSoDto();

        ViewBag.HocVienName = _service.GetHocVienNameByUserId(userId.Value);
        ViewBag.HangOptions = new SelectList(_service.GetHangGplxOptions(), "HangId", "TenHang", model.HangId);
        ViewBag.LoaiHoSoOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "Đăng ký mới", Text = "Đăng ký mới" },
            new SelectListItem { Value = "Nâng hạng", Text = "Nâng hạng" }
        };

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _service.CreateHoSoAsync(userId.Value, model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = "Tạo hồ sơ thành công";
        return RedirectToAction("Index");
    }
}