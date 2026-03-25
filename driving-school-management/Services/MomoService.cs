using driving_school_management.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace driving_school_management.Services
{
    public interface IMomoService
    {
        Task<string> CreatePaymentUrl(
            PaymentGatewayDto phieu,
            string returnUrl,
            string ipnUrl,
            string fakeReturnUrl);

        Task<MomoResult> ProcessReturn(IQueryCollection query);
    }
    public class MomoService : IMomoService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public MomoService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> CreatePaymentUrl(
            PaymentGatewayDto phieu,
            string returnUrl,
            string ipnUrl,
            string fakeReturnUrl)
        {
            bool sandbox = _config.GetValue<bool>("MOMO:UseSandbox");

            if (sandbox)
                return fakeReturnUrl;

            string endpoint = _config["MOMO:Endpoint"]!;
            string partnerCode = _config["MOMO:PartnerCode"]!;
            string accessKey = _config["MOMO:AccessKey"]!;
            string secretKey = _config["MOMO:SecretKey"]!;

            long amount = (long)phieu.TongTien;

            string orderId = "ORDER" + DateTime.Now.Ticks;
            string requestId = Guid.NewGuid().ToString();
            string orderInfo = $"Thanh toán khóa học {phieu.TenKhoaHoc}";
            string extraData = phieu.PhieuId.ToString();
            string requestType = "captureWallet";

            string raw =
                $"accessKey={accessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&ipnUrl={ipnUrl}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&partnerCode={partnerCode}" +
                $"&redirectUrl={returnUrl}" +
                $"&requestId={requestId}" +
                $"&requestType={requestType}";

            string signature = HmacSHA256(raw, secretKey);

            var payload = new
            {
                partnerCode,
                partnerName = "Driving School",
                storeId = "DRIVING-SCHOOL",
                requestId,
                orderId,
                amount,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl,
                lang = "vi",
                requestType,
                autoCapture = true,
                extraData,
                signature
            };

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync(
                $"{endpoint}/v2/gateway/api/create",
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            );

            string json = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(json)!;

            if (data.resultCode != 0)
                throw new Exception($"MoMo error: {data.message}");

            return data.payUrl.ToString();
        }

        public async Task<MomoResult> ProcessReturn(IQueryCollection query)
        {
            await Task.CompletedTask;

            string partnerCode = query["partnerCode"]!;
            string orderId = query["orderId"]!;
            string requestId = query["requestId"]!;
            string amount = query["amount"]!;
            string orderInfo = query["orderInfo"]!;
            string orderType = query["orderType"]!;
            string transId = query["transId"]!;
            string resultCode = query["resultCode"]!;
            string message = query["message"]!;
            string payType = query["payType"]!;
            string responseTime = query["responseTime"]!;
            string extraData = query["extraData"]!;
            string signature = query["signature"]!;

            string accessKey = _config["MOMO:AccessKey"]!;
            string secretKey = _config["MOMO:SecretKey"]!;

            string raw =
                $"accessKey={accessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&message={message}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&orderType={orderType}" +
                $"&partnerCode={partnerCode}" +
                $"&payType={payType}" +
                $"&requestId={requestId}" +
                $"&responseTime={responseTime}" +
                $"&resultCode={resultCode}" +
                $"&transId={transId}";

            string computed = HmacSHA256(raw, secretKey);

            if (!string.Equals(computed, signature, StringComparison.OrdinalIgnoreCase))
            {
                return new MomoResult
                {
                    Success = false,
                    Message = "Sai chữ ký MoMo"
                };
            }

            if (resultCode != "0")
            {
                return new MomoResult
                {
                    Success = false,
                    Message = "Thanh toán thất bại"
                };
            }

            if (!int.TryParse(extraData, out int phieuId))
            {
                return new MomoResult
                {
                    Success = false,
                    Message = "Không đọc được mã phiếu thanh toán"
                };
            }

            return new MomoResult
            {
                Success = true,
                PhieuId = phieuId
            };
        }

        private string HmacSHA256(string input, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return BitConverter.ToString(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(input))
            ).Replace("-", "").ToLower();
        }
    }
    public class MomoResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? PhieuId { get; set; }
    }
}