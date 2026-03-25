using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Globalization;

namespace driving_school_management.Services
{
    public interface IPayPalService
    {
        Task<string?> CreateOrderAsync(decimal amount, string currency, string returnUrl, string cancelUrl);
        Task<bool> CaptureOrderAsync(string orderId);
    }
    public class PayPalService : IPayPalService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public PayPalService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        private string GetBaseUrl()
        {
            string mode = _config["PayPal:Mode"] ?? "sandbox";
            return mode == "live"
                ? "https://api-m.paypal.com"
                : "https://api-m.sandbox.paypal.com";
        }

        private async Task<string> GetAccessToken()
        {
            var clientId = _config["PayPal:ClientId"];
            var clientSecret = _config["PayPal:ClientSecret"];

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{GetBaseUrl()}/v1/oauth2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" }
                })
            };

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("PayPal Auth Error: " + content);

            dynamic json = JsonConvert.DeserializeObject(content)!;
            return json.access_token.ToString();
        }

        public async Task<string?> CreateOrderAsync(decimal amount, string currency, string returnUrl, string cancelUrl)
        {
            var token = await GetAccessToken();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = currency,
                            value = amount.ToString("F2", CultureInfo.InvariantCulture)
                        }
                    }
                },
                application_context = new
                {
                    brand_name = "Driving School Payment",
                    landing_page = "LOGIN",
                    user_action = "PAY_NOW",
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{GetBaseUrl()}/v2/checkout/orders", content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("CreateOrder Error: " + result);

            dynamic data = JsonConvert.DeserializeObject(result)!;

            foreach (var link in data.links)
            {
                if (link.rel == "approve")
                    return link.href.ToString();
            }

            return null;
        }

        public async Task<bool> CaptureOrderAsync(string orderId)
        {
            var token = await GetAccessToken();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync(
                $"{GetBaseUrl()}/v2/checkout/orders/{orderId}/capture",
                new StringContent("{}", Encoding.UTF8, "application/json")
            );

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return false;

            dynamic data = JsonConvert.DeserializeObject(json)!;
            return data.status == "COMPLETED";
        }
    }
}