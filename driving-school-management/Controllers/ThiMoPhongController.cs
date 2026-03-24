using driving_school_management.Models.DTOs;
using driving_school_management.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace driving_school_management.Controllers
{
    public class ThiMoPhongController : Controller
    {
        private readonly IThiMoPhongService _thiMoPhongService;

        public ThiMoPhongController(IThiMoPhongService thiMoPhongService)
        {
            _thiMoPhongService = thiMoPhongService;
        }

        // ================================
        // AUTH HELPERS
        // ================================
        //private bool IsLoggedIn() => User?.Identity?.IsAuthenticated == true;
        private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId").HasValue;
        private int? TryGetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }
        //private int? TryGetCurrentUserId()
        //{
        //    var v = User.FindFirstValue("UserId");
        //    v ??= User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    if (int.TryParse(v, out var id)) return id;
        //    return null;
        //}

        // ================================
        // HELPERS
        // ================================
        private static List<FlagItem> NormalizeFlags(List<FlagItem> flags)
        {
            return flags
                .Where(f => f != null)
                .GroupBy(f => f.IdThMp)
                .Select(g => g.OrderBy(x => x.TimeSec).First())
                .ToList();
        }

        private static string BuildFlagsText(List<FlagItem> flags)
        {
            var normalized = NormalizeFlags(flags ?? new List<FlagItem>());
            return string.Join(";", normalized.Select(x => $"{x.IdThMp}|{x.TimeSec.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
        }

        private static string BuildSelectedIdsText(List<int> ids)
        {
            return string.Join(",", ids.Distinct());
        }

        private string NormalizeStaticPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "";

            path = path.Trim();

            if (path.StartsWith("wwwroot", StringComparison.OrdinalIgnoreCase))
                path = path.Substring("wwwroot".Length);

            path = path.Replace("\\", "/");

            if (!path.StartsWith("/"))
                path = "/" + path;

            return path;
        }

        // ================================
        // DANH SÁCH BỘ ĐỀ
        // ================================
        public async Task<IActionResult> DanhSachBoDe()
        {
            var userId = TryGetCurrentUserId();
            var dt = await _thiMoPhongService.GetDanhSachBoDeAsync(userId);

            var dsBoDe = new List<BoDeMoPhongViewModel>();

            foreach (DataRow row in dt.Rows)
            {
                dsBoDe.Add(new BoDeMoPhongViewModel
                {
                    IdBoDe = Convert.ToInt32(row["IDBODE"]),
                    TenBoDe = row["TENBODE"]?.ToString() ?? "",
                    SoTinhHuong = Convert.ToInt32(row["SOTINHHUONG"]),
                    HasResult = Convert.ToInt32(row["HASRESULT"]) == 1,
                    TongDiem = Convert.ToInt32(row["TONGDIEM"]),
                    KetQua = Convert.ToInt32(row["KETQUA"]) == 1,
                    SoTinhHuongSai = Convert.ToInt32(row["SOTINHHUONGSAI"]),
                    IdBaiLamMoiNhat = row["IDBAILAMMOINHAT"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(row["IDBAILAMMOINHAT"])
                });
            }

            return View(dsBoDe);
        }

        // ================================
        // LỊCH SỬ BÀI LÀM (CHỈ XEM)
        // ================================
        public async Task<IActionResult> LichSuBaiLam(int idBaiLam)
        {
            var dtHeader = await _thiMoPhongService.GetLichSuHeaderAsync(idBaiLam);
            if (dtHeader.Rows.Count == 0)
                return NotFound();

            var h = dtHeader.Rows[0];

            var vm = new LichSuMoPhongViewModel
            {
                IdBaiLam = Convert.ToInt32(h["IDBAILAM"]),
                IdBoDe = Convert.ToInt32(h["IDBODE"]),
                TongDiem = Convert.ToInt32(h["TONGDIEM"]),
                KetQua = Convert.ToInt32(h["KETQUA"]) == 1
            };

            var dtDetail = await _thiMoPhongService.GetLichSuChiTietAsync(idBaiLam);

            foreach (DataRow row in dtDetail.Rows)
            {
                vm.TinhHuongs.Add(new TinhHuongItem2
                {
                    IdThMp = Convert.ToInt32(row["IDTHMP"]),
                    TieuDe = row["TIEUDE"]?.ToString() ?? "",
                    VideoUrl = NormalizeStaticPath(row["VIDEOURL"]?.ToString()),
                    ScoreStartSec = Convert.ToDouble(row["SCORESTARTSEC"]),
                    ScoreEndSec = Convert.ToDouble(row["SCOREENDSEC"]),
                    HintImageUrl = NormalizeStaticPath(row["HINTIMAGEURL"]?.ToString())
                });

                if (row["TIMESEC"] != DBNull.Value)
                {
                    vm.Flags.Add(new ReviewFlagItem
                    {
                        IdThMp = Convert.ToInt32(row["IDTHMP"]),
                        TimeSec = Convert.ToDouble(row["TIMESEC"])
                    });
                }
            }

            return View(vm);
        }

        // ================================
        // LÀM BÀI THI
        // ================================
        public async Task<IActionResult> LamBai(int idBoDe)
        {
            var dt = await _thiMoPhongService.GetBoDeChiTietAsync(idBoDe);
            if (dt.Rows.Count == 0)
                return NotFound();

            var vm = new ThiTrialViewModel
            {
                IdBoDe = idBoDe
            };

            foreach (DataRow row in dt.Rows)
            {
                double startSec = Convert.ToDouble(row["SCORESTARTSEC"]);
                double endSec = Convert.ToDouble(row["SCOREENDSEC"]);

                var item = new TinhHuongItem2
                {
                    IdThMp = Convert.ToInt32(row["IDTHMP"]),
                    TieuDe = row["TIEUDE"]?.ToString() ?? "",
                    VideoUrl = NormalizeStaticPath(row["VIDEOURL"]?.ToString()),
                    ScoreStartSec = startSec,
                    ScoreEndSec = endSec,
                    HintImageUrl = NormalizeStaticPath(row["HINTIMAGEURL"]?.ToString())
                };

                double step = (endSec - startSec) / 5.0;
                item.Mocs.Clear();

                for (int i = 0; i < 5; i++)
                {
                    item.Mocs.Add(new MocDiemItem
                    {
                        Diem = 5 - i,
                        TimeSec = startSec + step * i
                    });
                }

                vm.TinhHuongs.Add(item);
            }

            return View(vm);
        }

        // ================================
        // LƯU KẾT QUẢ (Guest: không lưu DB, User: lưu DB)
        // ================================
        [HttpPost]
        public async Task<IActionResult> LuuKetQua([FromBody] KetQuaRequest request)
        {
            if (request == null) return BadRequest("Request null.");
            if (request.IdBoDe <= 0) return BadRequest("IdBoDe không hợp lệ.");
            if (request.Flags == null) request.Flags = new List<FlagItem>();

            var flagsText = BuildFlagsText(request.Flags);

            if (!IsLoggedIn())
            {
                var resultGuest = await _thiMoPhongService.ChamBoDeAsync(request.IdBoDe, flagsText);

                return Ok(new
                {
                    success = true,
                    tongDiem = resultGuest.TongDiem,
                    dat = resultGuest.Dat,
                    isGuest = true
                });
            }

            var userId = TryGetCurrentUserId();
            if (userId == null)
                return Unauthorized("Không lấy được UserId từ Claims.");

            var result = await _thiMoPhongService.LuuKetQuaAsync(userId.Value, request.IdBoDe, flagsText);

            return Ok(new
            {
                success = true,
                tongDiem = result.TongDiem,
                dat = result.Dat,
                isGuest = false,
                idBaiLam = result.IdBaiLam
            });
        }

        // ================================
        // KẾT QUẢ
        // ================================
        public async Task<IActionResult> KetQua(int id)
        {
            var dtHeader = await _thiMoPhongService.GetKetQuaHeaderAsync(id);
            if (dtHeader.Rows.Count == 0)
                return NotFound();

            var h = dtHeader.Rows[0];

            var vm = new KetQuaThiViewModel
            {
                TongDiem = Convert.ToInt32(h["TONGDIEM"]),
                KetQua = Convert.ToInt32(h["KETQUA"]) == 1
            };

            var dtDetail = await _thiMoPhongService.GetKetQuaChiTietAsync(id);

            foreach (DataRow row in dtDetail.Rows)
            {
                vm.ChiTiet.Add(new ChiTietKetQuaItem
                {
                    TieuDe = row["TIEUDE"]?.ToString() ?? "",
                    ThoiDiemNhan = Convert.ToDouble(row["THOIDIEMNHAN"]),
                    Diem = Convert.ToInt32(row["DIEM"])
                });
            }

            return View(vm);
        }

        // ================================
        // LÀM BÀI NGẪU NHIÊN (10 tình huống)
        // ================================
        public async Task<IActionResult> LamBaiNgauNhien()
        {
            var dt = await _thiMoPhongService.GetDeNgauNhienAsync();

            var vm = new ThiTrialViewModel
            {
                IdBoDe = 0
            };

            foreach (DataRow row in dt.Rows)
            {
                vm.TinhHuongs.Add(new TinhHuongItem2
                {
                    IdThMp = Convert.ToInt32(row["IDTHMP"]),
                    TieuDe = row["TIEUDE"]?.ToString() ?? "",
                    VideoUrl = NormalizeStaticPath(row["VIDEOURL"]?.ToString()),
                    ScoreStartSec = Convert.ToDouble(row["SCORESTARTSEC"]),
                    ScoreEndSec = Convert.ToDouble(row["SCOREENDSEC"]),
                    HintImageUrl = NormalizeStaticPath(row["HINTIMAGEURL"]?.ToString()),
                    Kho = Convert.ToInt32(row["KHO"]) == 1
                });
            }

            return View("LamBai", vm);
        }

        // ================================
        // CHẤM ĐIỂM ĐỀ NGẪU NHIÊN
        // - Không lưu DB
        // ================================
        [HttpPost]
        public async Task<IActionResult> LuuKetQuaNgauNhien([FromBody] RandomKetQuaRequest request)
        {
            if (request == null) return BadRequest("Request null.");
            if (request.SelectedThIds == null || request.SelectedThIds.Count == 0)
                return BadRequest("Thiếu SelectedThIds.");

            var selectedIdsText = BuildSelectedIdsText(request.SelectedThIds);
            var flagsText = BuildFlagsText(request.Flags ?? new List<FlagItem>());

            var result = await _thiMoPhongService.ChamDeNgauNhienAsync(selectedIdsText, flagsText);

            return Ok(new
            {
                success = true,
                tongDiem = result.TongDiem,
                dat = result.Dat,
                isRandom = true
            });
        }
    }
}
