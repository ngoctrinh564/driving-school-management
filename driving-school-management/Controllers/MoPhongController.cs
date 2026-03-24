using driving_school_management.Models;
using driving_school_management.Services;
using driving_school_management.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace driving_school_management.Controllers
{
    public class MoPhongController : Controller
    {
        private readonly IMoPhongService _moPhongService;

        public MoPhongController(IMoPhongService moPhongService)
        {
            _moPhongService = moPhongService;
        }

        // ================================
        // ÔN TẬP TẤT CẢ TÌNH HUỐNG MÔ PHỎNG
        // ================================
        public async Task<IActionResult> Index()
        {
            var dt = await _moPhongService.GetOnTapMoPhongAsync();

            var chuongDict = new Dictionary<int, ChuongMoPhongVm>();

            foreach (DataRow row in dt.Rows)
            {
                int idChuong = Convert.ToInt32(row["IDCHUONGMP"]);

                if (!chuongDict.ContainsKey(idChuong))
                {
                    chuongDict[idChuong] = new ChuongMoPhongVm
                    {
                        IdChuongMp = idChuong,
                        TenChuong = row["TENCHUONG"]?.ToString() ?? "",
                        TinhHuongs = new List<TinhHuongItem2>()
                    };
                }

                if (row["IDTHMP"] != DBNull.Value)
                {
                    chuongDict[idChuong].TinhHuongs.Add(new TinhHuongItem2
                    {
                        IdThMp = Convert.ToInt32(row["IDTHMP"]),
                        TieuDe = string.IsNullOrWhiteSpace(row["TIEUDE"]?.ToString())
                            ? $"Tình huống #{Convert.ToInt32(row["IDTHMP"])}"
                            : row["TIEUDE"]!.ToString()!,
                        VideoUrl = NormalizeStaticPath(row["VIDEOURL"]?.ToString()),
                        HintImageUrl = NormalizeStaticPath(row["HINTIMAGEURL"]?.ToString()),
                        ScoreStartSec = Convert.ToDouble(row["SCORESTARTSEC"]),
                        ScoreEndSec = Convert.ToDouble(row["SCOREENDSEC"]),
                        Kho = Convert.ToInt32(row["KHO"]) == 1
                    });
                }
            }

            var vm = new OnTapMoPhongViewModel
            {
                Chuongs = chuongDict.Values.ToList()
            };

            return View(vm);
        }

        // ================================
        // HELPERS
        // ================================
        private static string NormalizeStaticPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";

            path = path.Trim();

            if (path.StartsWith("wwwroot", StringComparison.OrdinalIgnoreCase))
                path = path.Substring("wwwroot".Length);

            path = path.Replace("\\", "/");

            if (!path.StartsWith("/"))
                path = "/" + path;

            return path;
        }
    }
}