using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using driving_school_management.Services;

namespace driving_school_management.Controllers
{
    public class AdminKhoaHocController : Controller
    {
        private readonly AdminKhoaHocService _adminKhoaHocService;
        private readonly IConfiguration _configuration;

        public AdminKhoaHocController(AdminKhoaHocService adminKhoaHocService, IConfiguration configuration)
        {
            _adminKhoaHocService = adminKhoaHocService;
            _configuration = configuration;
        }

        public IActionResult Index(string? keyword, int? hangId, string? trangThai, int page = 1)
        {
            var result = _adminKhoaHocService.GetList(keyword, hangId, trangThai, page);

            ViewBag.Page = page;
            ViewBag.Total = result.Total;
            ViewBag.TotalPages = (int)Math.Ceiling((double)result.Total / 10);

            LoadHangs();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CoursesTable", result.Data);
            }

            return View(result.Data);
        }

        private void LoadHangs()
        {
            var hangs = new List<dynamic>();

            using var conn = new OracleConnection(_configuration.GetConnectionString("OracleDb"));
            using var cmd = new OracleCommand("SELECT hangId, tenHang FROM HangGplx ORDER BY tenHang", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                hangs.Add(new
                {
                    hangId = reader["HANGID"],
                    tenHang = reader["TENHANG"]?.ToString()
                });
            }

            ViewBag.Hangs = hangs;
        }
    }
}