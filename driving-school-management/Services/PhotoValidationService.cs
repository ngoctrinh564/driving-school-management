using driving_school_management.ViewModels;
using driving_school_management.ViewModels.driving_school_management.ViewModels;
using Microsoft.AspNetCore.Http;
using OpenCvSharp;

namespace driving_school_management.Services
{
    public interface IPhotoValidationService
    {
        Task<PhotoValidationResultVM> ValidatePhotoAsync(IFormFile? file);
    }
    public class PhotoValidationService : IPhotoValidationService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

        public PhotoValidationService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<PhotoValidationResultVM> ValidatePhotoAsync(IFormFile? file)
        {
            var result = new PhotoValidationResultVM();

            if (file == null)
            {
                result.Reasons.Add("Không có file ảnh.");
                result.IsValid = false;
                return result;
            }

            if (file.Length <= 0)
            {
                result.Reasons.Add("File ảnh rỗng.");
                result.IsValid = false;
                return result;
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension) || !_allowedExtensions.Contains(extension))
            {
                result.Reasons.Add("Chỉ chấp nhận định dạng jpg, jpeg, png.");
                result.IsValid = false;
                return result;
            }

            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            if (imageBytes.Length == 0)
            {
                result.Reasons.Add("Không đọc được dữ liệu ảnh.");
                result.IsValid = false;
                return result;
            }

            var cascadePath = Path.Combine(_environment.WebRootPath, "assets", "haarcascade_frontalface_default.xml");
            if (!System.IO.File.Exists(cascadePath))
            {
                result.Reasons.Add("Không tìm thấy file haarcascade_frontalface_default.xml.");
                result.IsValid = false;
                return result;
            }

            try
            {
                using var image = Cv2.ImDecode(imageBytes, ImreadModes.Color);

                if (image.Empty())
                {
                    result.Reasons.Add("File upload không phải ảnh hợp lệ.");
                    result.IsValid = false;
                    return result;
                }

                result.ImageWidth = image.Width;
                result.ImageHeight = image.Height;

                if (image.Width < 400 || image.Height < 400)
                {
                    result.Reasons.Add("Kích thước ảnh tối thiểu phải từ 400x400px.");
                }

                using var gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                // Độ sáng trung bình
                result.Brightness = Cv2.Mean(gray).Val0;
                if (result.Brightness < 50 || result.Brightness > 200)
                {
                    result.Reasons.Add("Độ sáng ảnh phải nằm trong khoảng từ 50 đến 200.");
                }

                // Độ nét bằng Laplacian variance
                using var laplacian = new Mat();
                Cv2.Laplacian(gray, laplacian, MatType.CV_64F);
                Cv2.MeanStdDev(laplacian, out _, out var stddev);
                result.BlurScore = stddev.Val0 * stddev.Val0;

                if (result.BlurScore < 100)
                {
                    result.Reasons.Add("Ảnh bị mờ, blur score phải >= 100.");
                }

                using var classifier = new CascadeClassifier(cascadePath);
                var faces = classifier.DetectMultiScale(
                    gray,
                    scaleFactor: 1.1,
                    minNeighbors: 5,
                    flags: HaarDetectionTypes.ScaleImage,
                    minSize: new Size(30, 30)
                );

                result.FaceCount = faces.Length;

                if (faces.Length != 1)
                {
                    result.Reasons.Add("Ảnh phải có đúng 1 khuôn mặt.");
                }

                if (faces.Length == 1)
                {
                    var face = faces[0];

                    result.FaceBox = new FaceBoxVM
                    {
                        X = face.X,
                        Y = face.Y,
                        Width = face.Width,
                        Height = face.Height
                    };

                    result.FaceRatio = (double)face.Width / image.Width;

                    if (result.FaceRatio < 0.4 || result.FaceRatio > 0.75)
                    {
                        result.Reasons.Add("Tỉ lệ khuôn mặt phải nằm trong khoảng từ 0.4 đến 0.75 theo chiều rộng ảnh.");
                    }

                    var imageCenterX = image.Width / 2.0;
                    var imageCenterY = image.Height / 2.0;
                    var faceCenterX = face.X + face.Width / 2.0;
                    var faceCenterY = face.Y + face.Height / 2.0;

                    var offsetX = Math.Abs(faceCenterX - imageCenterX);
                    var offsetY = Math.Abs(faceCenterY - imageCenterY);

                    var maxOffsetX = image.Width * 0.15;
                    var maxOffsetY = image.Height * 0.15;

                    if (offsetX > maxOffsetX || offsetY > maxOffsetY)
                    {
                        result.Reasons.Add("Tâm khuôn mặt phải gần trung tâm ảnh, sai lệch tối đa 15% chiều rộng và chiều cao.");
                    }
                }

                result.IsValid = result.Reasons.Count == 0;
                return result;
            }
            catch
            {
                result.Reasons.Add("Lỗi khi xử lý ảnh bằng OpenCvSharp.");
                result.IsValid = false;
                return result;
            }
        }
    }
}