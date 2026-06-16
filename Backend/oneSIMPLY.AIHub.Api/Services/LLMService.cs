using System.Text;
using System.Text.Json;
using oneSIMPLY.AIHub.Api.Models;

namespace oneSIMPLY.AIHub.Api.Services;

public interface ILLMService
{
    Task<string?> CallLLMAsync(string prompt, string model);
}

public class LLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly string _openAiKey;
    private readonly string _deepSeekKey;

    public LLMService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _openAiKey = config["AI:OpenAIKey"] ?? "";
        _deepSeekKey = config["AI:DeepSeekKey"] ?? "";
    }

    public async Task<string?> CallLLMAsync(string prompt, string model)
    {
        try
        {
            if (model.Contains("gpt") && string.IsNullOrWhiteSpace(_openAiKey))
                return BuildDemoResponse(prompt, model);

            if (!model.Contains("gpt") && string.IsNullOrWhiteSpace(_deepSeekKey))
                return BuildDemoResponse(prompt, model);

            string url;
            string apiKey;
            object payload;

            if (model.Contains("gpt"))
            {
                url = "https://api.openai.com/v1/chat/completions";
                apiKey = _openAiKey;
                payload = new
                {
                    model,
                    messages = new[] { new { role = "user", content = prompt } },
                    temperature = 0.7
                };
            }
            else
            {
                url = "https://api.deepseek.com/v1/chat/completions";
                apiKey = _deepSeekKey;
                payload = new
                {
                    model = "deepseek-chat",
                    messages = new[] { new { role = "user", content = prompt } },
                    temperature = 0.7
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            int retryCount = 0;
            HttpResponseMessage? response = null;
            while (retryCount < 3)
            {
                response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) break;
                retryCount++;
                await Task.Delay((int)Math.Pow(2, retryCount) * 1000);
            }

            if (response == null || !response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        catch
        {
            return "Máy chủ AI đối tác tạm thời quá tải. Vui lòng thử lại sau ít phút!";
        }
    }

    private static string BuildDemoResponse(string prompt, string model)
    {
        var productLine = prompt.Split('\n').FirstOrDefault(l => l.Contains("Tên sản phẩm")) ?? "Sản phẩm của bạn";
        return $"""
            🔥 {productLine.Replace("- Tên sản phẩm/dịch vụ:", "").Trim()} — Giải pháp bạn đang tìm kiếm!

            ✨ Bạn có biết? Hàng ngàn khách hàng đã tin dùng và hài lòng với chất lượng vượt trội.

            💎 Điểm nổi bật:
            • Chất lượng cao cấp, giá cả hợp lý
            • Giao hàng nhanh, hỗ trợ tận tâm 24/7
            • Cam kết hoàn tiền nếu không hài lòng

            🛒 INBOX NGAY hoặc comment "TƯ VẤN" để nhận ưu đãi đặc biệt hôm nay!

            #onesimply #marketing #banhang

            ---
            [Chế độ DEMO — Model: {model}. Điền API Key trong appsettings.json để dùng AI thật.]
            """;
    }
}
