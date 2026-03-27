using Microsoft.AspNetCore.Mvc;
using driving_school_management.Services;
using driving_school_management.ViewModels;

namespace driving_school_management.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPhotoValidationService _photoValidationService;

        public UserController(IAuthService authService, IPhotoValidationService photoValidationService)
        {
            _authService = authService;
            _photoValidationService = photoValidationService;
        }

        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var data = _authService.GetUserDashboard(userId.Value);

            if (data == null)
            {
                TempData["Error"] = "Không tìm thấy dữ liệu";
                return RedirectToAction("Index", "Home");
            }

            return View(data);
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
        public IActionResult Edit(EditUserVM model, IFormFile? avatarFile)
        {
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                var current = _authService.GetUserProfile(model.UserId);
                if (current != null)
                {
                    model.Username = current.Username;
                }
                ModelState.Remove(nameof(model.Username));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value!.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(" | ", x.Value!.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();

                TempData["Error"] = "ModelState không hợp lệ: " + string.Join(" || ", errors);
                return View(model);
            }

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

            int result = _authService.UpdateUserProfile(model);

            if (result == 1)
            {
                TempData["Success"] = "Cập nhật thông tin thành công";
                return RedirectToAction("Index", "User");
            }

            if (result == -1)
            {
                TempData["Error"] = "Tên đăng nhập đã tồn tại";
                return View(model);
            }

            if (result == -2)
            {
                TempData["Error"] = "Email đã tồn tại";
                return View(model);
            }

            TempData["Error"] = "Cập nhật thất bại";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ValidateAvatar(IFormFile? avatarFile)
        {
            var result = await _photoValidationService.ValidatePhotoAsync(avatarFile);
            return Json(result);
        }
    }
}   