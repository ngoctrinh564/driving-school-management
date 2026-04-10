using driving_school_management.Services;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    [Route("MyCourse")]
    public class MyCourseController : Controller
    {
        private readonly KhoaHocService _service;

        public MyCourseController(KhoaHocService service)
        {
            _service = service;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetMyCourses")]
        public IActionResult GetMyCourses()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Phiên đăng nhập không hợp lệ"
                });
            }

            try
            {
                var data = _service.GetMyCourses(userId.Value);
                return Json(new
                {
                    success = true,
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("GetMyCourseDetail")]
        public IActionResult GetMyCourseDetail(int khoaHocId)
        {
            if (khoaHocId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "khoaHocId không hợp lệ"
                });
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Phiên đăng nhập không hợp lệ"
                });
            }

            try
            {
                var data = _service.GetMyCourseDetail(userId.Value, khoaHocId);
                if (data == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy chi tiết khóa học"
                    });
                }

                return Json(new
                {
                    success = true,
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}