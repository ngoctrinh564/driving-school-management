using Microsoft.AspNetCore.Mvc;
using driving_school_management.Services;
using driving_school_management.ViewModels;

namespace driving_school_management.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthService _authService;

        public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var profile = _authService.GetUserProfile(userId.Value);
            if (profile == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin user";
                return RedirectToAction("Index", "Home");
            }

            return View(profile);
        }

        [HttpGet]
        public IActionResult Edit()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var profile = _authService.GetUserProfile(userId.Value);
            if (profile == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin user";
                return RedirectToAction("Index", "Home");
            }

            var vm = new EditUserVM
            {
                UserId = profile.UserId,
                HocVienId = profile.HocVienId,
                Username = profile.Username,
                Email = profile.Email,
                HoTen = profile.HoTen,
                SoCmndCccd = profile.SoCmndCccd,
                NamSinh = profile.NamSinh,
                GioiTinh = profile.GioiTinh,
                Sdt = profile.Sdt,
                AvatarUrl = profile.AvatarUrl
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(EditUserVM model, IFormFile avatarFile)
        {
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatar");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    avatarFile.CopyTo(stream);
                }

                model.AvatarUrl = "/images/avatar/" + fileName;
            }

            if (!ModelState.IsValid)
                return View(model);

            int result = _authService.UpdateUserProfile(model);

            if (result == 1)
            {
                HttpContext.Session.SetString("Username", model.Username);
                TempData["Success"] = "Cập nhật thông tin thành công";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Cập nhật thất bại";
            return View(model);
        }
    }
}   