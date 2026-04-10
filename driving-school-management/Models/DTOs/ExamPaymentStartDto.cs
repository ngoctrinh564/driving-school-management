namespace driving_school_management.Models.DTOs
{
    public class ExamPaymentStartDto
    {
        public List<PaymentGatewayDto> PhieuList { get; set; } = new();
        public decimal TongTien { get; set; }
    }
}
