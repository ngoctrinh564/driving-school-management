using driving_school_management.Helpers;
using driving_school_management.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace driving_school_management.Controllers
{
    public class AdminSignsController : Controller
    {
        private readonly SignOracleHelper _signOracleHelper;
        private readonly IWebHostEnvironment _environment;

        public AdminSignsController(SignOracleHelper signOracleHelper, IWebHostEnvironment environment)
        {
            _signOracleHelper = signOracleHelper;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var data = _signOracleHelper.GetAll();
            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SignVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SignVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.TENBIENBAO))
            {
                ModelState.AddModelError(nameof(vm.TENBIENBAO), "Tên biển báo không được để trống.");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(vm.ImageFile.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("ImageFile", "Chỉ cho phép ảnh định dạng JPG, JPEG, PNG, GIF, WEBP.");
                    return View(vm);
                }

                var newSignsFolder = Path.Combine(_environment.WebRootPath, "images", "new-signs");
                Directory.CreateDirectory(newSignsFolder);

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(newSignsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    vm.ImageFile.CopyTo(stream);
                }

                vm.HINHANH = $"/images/new-signs/{fileName}";
            }

            _signOracleHelper.Insert(vm);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var item = _signOracleHelper.GetById(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SignVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.TENBIENBAO))
            {
                ModelState.AddModelError(nameof(vm.TENBIENBAO), "Tên biển báo không được để trống.");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var oldItem = _signOracleHelper.GetById(vm.IDBIENBAO);
            if (oldItem == null) return NotFound();

            vm.HINHANH = oldItem.HINHANH;

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(vm.ImageFile.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("ImageFile", "Chỉ cho phép ảnh định dạng JPG, JPEG, PNG, GIF, WEBP.");
                    return View(vm);
                }

                var newSignsFolder = Path.Combine(_environment.WebRootPath, "images", "new-signs");
                Directory.CreateDirectory(newSignsFolder);

                string fileName;

                if (!string.IsNullOrEmpty(oldItem.HINHANH))
                {
                    fileName = Path.GetFileName(oldItem.HINHANH);

                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        fileName = $"{Guid.NewGuid():N}{ext}";
                    }
                    else
                    {
                        var oldExt = Path.GetExtension(fileName);
                        if (!string.Equals(oldExt, ext, StringComparison.OrdinalIgnoreCase))
                        {
                            fileName = Path.GetFileNameWithoutExtension(fileName) + ext;
                        }
                    }
                }
                else
                {
                    fileName = $"{Guid.NewGuid():N}{ext}";
                }

                var newFilePath = Path.Combine(newSignsFolder, fileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    vm.ImageFile.CopyTo(stream);
                }

                vm.HINHANH = $"/images/new-signs/{fileName}";
            }

            _signOracleHelper.Update(vm);
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var item = _signOracleHelper.GetById(id);
            if (item == null) return NotFound();

            if (!string.IsNullOrEmpty(item.HINHANH) &&
                item.HINHANH.StartsWith("/images/new-signs/", StringComparison.OrdinalIgnoreCase))
            {
                var oldPath = Path.Combine(
                    _environment.WebRootPath,
                    item.HINHANH.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            _signOracleHelper.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}