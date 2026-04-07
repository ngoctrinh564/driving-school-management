using driving_school_management.Models.DTOs;
using driving_school_management.Services;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    public class AdminExamController : Controller
    {
        private readonly AdminExamService _service;
        private const int PageSize = 10;

        public AdminExamController(AdminExamService service)
        {
            _service = service;
        }

        // ===================== DANH SÁCH KỲ THI =====================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var data = await _service.GetKyThiAsync();
            return View(data);
        }

        // ===================== TẠO KỲ THI =====================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.HangList = await _service.GetHangGplxAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int hangId, int dot, int nam)
        {
            try
            {
                await _service.CreateKyThiAsync(hangId, dot, nam);
                TempData["Success"] = "Tạo kỳ thi thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.HangList = await _service.GetHangGplxAsync();
                TempData["Error"] = ex.Message;
                return View();
            }
        }

        // ===================== SỬA KỲ THI =====================
        [HttpGet]
        public async Task<IActionResult> Edit(int kyThiId)
        {
            try
            {
                var item = await _service.GetKyThiEditInfoAsync(kyThiId);
                if (item == null)
                {
                    return NotFound();
                }

                ViewBag.HangList = await _service.GetHangGplxAsync();
                return View(item);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int kyThiId, int hangId, int dot, int nam)
        {
            try
            {
                await _service.UpdateCapKyThiAsync(kyThiId, hangId, dot, nam);
                TempData["Success"] = "Cập nhật cặp kỳ thi thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var item = await _service.GetKyThiEditInfoAsync(kyThiId);
                ViewBag.HangList = await _service.GetHangGplxAsync();
                TempData["Error"] = ex.Message;
                return View(item);
            }
        }

        // ===================== XÓA KỲ THI =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int kyThiId)
        {
            try
            {
                await _service.DeleteKyThiAsync(kyThiId);
                TempData["Success"] = "Xóa kỳ thi thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ===================== PAGE 1: LỊCH THI =====================
        [HttpGet]
        public async Task<IActionResult> Schedules(int kyThiId)
        {
            try
            {
                var kyThiList = await _service.GetKyThiAsync();
                var kyThi = kyThiList.FirstOrDefault(x => x.KyThiId == kyThiId);

                if (kyThi == null)
                {
                    return NotFound();
                }

                var lichThiList = await _service.GetLichThiByKyThiAsync(kyThiId);

                ViewBag.KyThiId = kyThiId;
                ViewBag.KyThi = kyThi;

                return View(lichThiList);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ===================== TỰ ĐỘNG PHÂN CÔNG LỊCH THI =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoAssignSchedules(int kyThiId)
        {
            try
            {
                await _service.AutoPhanCongLichThiAsync(kyThiId);
                TempData["Success"] = "Phân công lịch thi thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Schedules), new { kyThiId });
        }

        // ===================== PAGE 2: HỌC VIÊN ĐÃ ĐĂNG KÝ =====================
        [HttpGet]
        public async Task<IActionResult> RegisteredStudents(int kyThiId, string? keyword, int page = 1)
        {
            try
            {
                if (page < 1)
                {
                    page = 1;
                }

                var kyThiList = await _service.GetKyThiAsync();
                var kyThi = kyThiList.FirstOrDefault(x => x.KyThiId == kyThiId);

                if (kyThi == null)
                {
                    return NotFound();
                }

                var totalItems = await _service.GetHocVienDangKyCountAsync(kyThiId, keyword);
                var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

                if (totalPages == 0)
                {
                    totalPages = 1;
                }

                if (page > totalPages)
                {
                    page = totalPages;
                }

                var students = await _service.GetHocVienDangKyAsync(kyThiId, keyword, page, PageSize);

                ViewBag.KyThiId = kyThiId;
                ViewBag.KyThi = kyThi;
                ViewBag.Keyword = keyword;
                ViewBag.Page = page;
                ViewBag.PageSize = PageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.TotalPages = totalPages;

                return View(students);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}