using driving_school_management.Services;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // ================= LOGIN =================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await _authService.Login(username, password);

            if (result == null)
            {
                TempData["Error"] = "Sai tài khoản hoặc mật khẩu";
                return RedirectToAction("Login");
            }

            if (!result.IsActive)
            {
                TempData["Error"] = "Tài khoản bị khóa";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetInt32("UserId", result.UserId);
            HttpContext.Session.SetString("Username", result.Username);
            HttpContext.Session.SetInt32("RoleId", result.RoleId);

            // CHECK HOCVIEN (EMAIL, SDT, CCCD)
            var isCompleted = await _authService.IsProfileCompleted(result.UserId);

            if (!isCompleted && result.RoleId != 1)
            {
                TempData["Warning"] = "Vui lòng cập nhật thông tin cá nhân";
                return RedirectToAction("Edit", "User");
            }

            if (result.RoleId == 1)
            {
                TempData["Success"] = "Admin: " + username + " đăng nhập thành công";
                return RedirectToAction("Index", "AdminDashboard");
            }

            TempData["Success"] = username + " đăng nhập thành công";
            return RedirectToAction("Index", "Home");
        }


        // ================= REGISTER =================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            var result = await _authService.Register(username, password);

            if (result == -1)
            {
                TempData["Error"] = "Username đã tồn tại";
                return RedirectToAction("Register");
            }

            // AUTO LOGIN SAU REGISTER
            var login = await _authService.Login(username, password);

            HttpContext.Session.SetInt32("UserId", login.UserId);
            HttpContext.Session.SetString("Username", login.Username);
            HttpContext.Session.SetInt32("RoleId", login.RoleId);

            TempData["Success"] = "Đăng ký thành công, vui lòng cập nhật thông tin";

            return RedirectToAction("Edit", "User");
        }

        // ================= RESET PASSWORD =================
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string username, string newPassword)
        {
            var result = await _authService.ResetPassword(username, newPassword);

            if (result == -1)
            {
                TempData["Error"] = "User không tồn tại";
                return RedirectToAction("Login");
            }

            TempData["Success"] = "Đổi mật khẩu thành công";
            return RedirectToAction("Login");
        }

        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = " Đã đăng xuất";
            return RedirectToAction("Login");
        }
    }
}