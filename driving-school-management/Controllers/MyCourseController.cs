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

        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetMyCourses")]
        public IActionResult GetMyCourses()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var data = _service.GetMyCourses(userId.Value);
            return Json(data);
        }

        [HttpGet("GetMyCourseDetail")]
        public IActionResult GetMyCourseDetail(int khoaHocId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var data = _service.GetMyCourseDetail(userId.Value, khoaHocId);
            if (data == null)
                return NotFound();

            return Json(data);
        }
    }
}