namespace driving_school_management.ViewModels
{
    namespace driving_school_management.ViewModels
    {
        public class FaceBoxVM
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public class PhotoValidationResultVM
        {
            public bool IsValid { get; set; }
            public List<string> Reasons { get; set; } = new List<string>();
            public int FaceCount { get; set; }
            public FaceBoxVM? FaceBox { get; set; }
            public int ImageWidth { get; set; }
            public int ImageHeight { get; set; }
            public double FaceRatio { get; set; }
            public double BlurScore { get; set; }
            public double Brightness { get; set; }
        }
    }
}
