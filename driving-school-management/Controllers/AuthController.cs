using driving_school_management.Helpers;
using driving_school_management.Services;
using driving_school_management.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
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

            var isCompleted = await _authService.IsProfileCompleted(result.UserId);

            if (!isCompleted && result.RoleId != 1)
            {
                TempData["Warning"] = "Vui lòng cập nhật thông tin cá nhân (*)";
                return RedirectToAction("Edit", "User");
            }

            if (result.RoleId == 1)
            {
                TempData["Success"] = "Admin đăng nhập thành công";
                return RedirectToAction("Index", "AdminDashboard");
            }

            TempData["Success"] = "Đăng nhập thành công";
            return RedirectToAction("Index", "Home");
        }

        // ================= REGISTER =================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Bước 1: Nhập thông tin → gửi OTP
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string otpCode = new Random().Next(100000, 999999).ToString();

            var otpSession = new RegisterOtpSessionVM
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password,
                RoleId = 2,
                OtpCode = otpCode,
                ExpiredAt = DateTime.Now.AddSeconds(60)
            };

            HttpContext.Session.SetObject("RegisterOtp", otpSession);

            await _emailService.SendOtpEmailAsync(model.Email, otpCode);

            TempData["Success"] = "Đã gửi OTP về email";
            return RedirectToAction("VerifyOtp");
        }

        // ================= VERIFY OTP =================
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var sessionData = HttpContext.Session.GetObject<RegisterOtpSessionVM>("RegisterOtp");

            if (sessionData == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin OTP";
                return RedirectToAction("Register");
            }

            var model = new VerifyOtpVM
            {
                Username = sessionData.Username,
                Email = sessionData.Email,
                Password = sessionData.Password,
                RoleId = sessionData.RoleId
            };

            return View(model);
        }

        // Bước 2: Nhập OTP → insert DB + auto login
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpVM model)
        {
            var sessionData = HttpContext.Session.GetObject<RegisterOtpSessionVM>("RegisterOtp");

            if (sessionData == null)
            {
                TempData["Error"] = "OTP đã hết hạn";
                return RedirectToAction("Register");
            }

            if (DateTime.Now > sessionData.ExpiredAt)
            {
                HttpContext.Session.Remove("RegisterOtp");
                TempData["Error"] = "OTP đã hết hạn";
                return RedirectToAction("Register");
            }

            if (sessionData.OtpCode != model.OtpCode)
            {
                TempData["Error"] = "OTP không đúng";
                return View(model);
            }

            int result = _authService.Register(
                sessionData.Username,
                sessionData.Password,
                sessionData.Email,
                sessionData.RoleId
            );

            if (result != 1)
            {
                TempData["Error"] = "Đăng ký thất bại (username đã tồn tại)";
                return RedirectToAction("Register");
            }

            var loginResult = await _authService.Login(sessionData.Email, sessionData.Password);

            if (loginResult == null)
            {
                TempData["Error"] = "Đăng nhập tự động thất bại";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetInt32("UserId", loginResult.UserId);
            HttpContext.Session.SetString("Username", loginResult.Username);
            HttpContext.Session.SetInt32("RoleId", loginResult.RoleId);

            HttpContext.Session.Remove("RegisterOtp");

            TempData["Success"] = "Đăng ký thành công. Vui lòng bổ sung thông tin (*)";

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
            TempData["Success"] = "Đã đăng xuất";
            return RedirectToAction("Login");
        }
    }
}