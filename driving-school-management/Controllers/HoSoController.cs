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

    // SỬA
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        var model = _service.GetEditHoSoByUser(id, userId.Value);
        if (model == null)
        {
            return NotFound();
        }

        ViewBag.HocVienName = _service.GetHocVienNameByUserId(userId.Value);
        ViewBag.HangOptions = new SelectList(_service.GetHangGplxOptions(), "HangId", "TenHang", model.HangId);
        ViewBag.LoaiHoSoOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "Đăng ký mới", Text = "Đăng ký mới" },
        new SelectListItem { Value = "Nâng hạng", Text = "Nâng hạng" }
    };

        return View(model);
    }

    [HttpPost]
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
    [RequestSizeLimit(104857600)]
    public async Task<IActionResult> Edit(EditHoSoDto model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.HocVienName = _service.GetHocVienNameByUserId(userId.Value);
        ViewBag.HangOptions = new SelectList(_service.GetHangGplxOptions(), "HangId", "TenHang", model.HangId);
        ViewBag.LoaiHoSoOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "Đăng ký mới", Text = "Đăng ký mới" },
            new SelectListItem { Value = "Nâng hạng", Text = "Nâng hạng" }
        };

        var oldData = _service.GetEditHoSoByUser(model.HoSoId, userId.Value);
        if (oldData != null && (model.NewImages == null || model.NewImages.Count == 0))
        {
            model.ExistingImages = oldData.ExistingImages;
            model.HoTen = oldData.HoTen;
            model.SoCmndCccd = oldData.SoCmndCccd;
            model.NamSinh = oldData.NamSinh;
            model.GioiTinh = oldData.GioiTinh;
            model.Sdt = oldData.Sdt;
            model.Email = oldData.Email;
            model.AvatarUrl = oldData.AvatarUrl;
            model.NgayDangKy = oldData.NgayDangKy;
            model.TrangThai = oldData.TrangThai;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _service.UpdateHoSoAsync(userId.Value, model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = "Cập nhật hồ sơ thành công";
        return RedirectToAction("Index");
    }
}