using System;
using System.Collections.Generic;

namespace driving_school_management.ViewModels
{
    public class DashboardReportVm
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public decimal TongDoanhThu { get; set; }
        public decimal SoThangCoDoanhThu { get; set; }
        public decimal SoHangGplx { get; set; }
        public decimal DoanhThuTrungBinhThang { get; set; }

        public decimal SoNguoiMoi { get; set; }
        public decimal SoKhoaHocMoi { get; set; }
        public decimal SoKyThiMoi { get; set; }

        public List<ChartPointVm> RevenueByMonth { get; set; } = new List<ChartPointVm>();
        public List<ChartPointVm> NewUsersByMonth { get; set; } = new List<ChartPointVm>();
        public List<ChartPointVm> NewCoursesByMonth { get; set; } = new List<ChartPointVm>();
        public List<PieSliceVm> CoursesByHang { get; set; } = new List<PieSliceVm>();
        public List<ChartPointVm> NewExamsByMonth { get; set; } = new List<ChartPointVm>();
        public List<PieSliceVm> ExamsByType { get; set; } = new List<PieSliceVm>();
    }
}