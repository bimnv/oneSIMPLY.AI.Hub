using System.Globalization;
using System.Text;
using oneSIMPLY.AIHub.Api.Models;

namespace oneSIMPLY.AIHub.Api.Services;

public static class PromptTemplateService
{
    public static string BuildCopywritingPrompt(string productName, string features, string tone, string formula, string? imageUrl)
    {
        var formulaName = formula.Equals("PAS", StringComparison.OrdinalIgnoreCase)
            ? "PAS (Problem - Agitate - Solution)"
            : "AIDA (Attention - Interest - Desire - Action)";

        var imageNote = string.IsNullOrWhiteSpace(imageUrl)
            ? "Không có ảnh đính kèm."
            : "Có ảnh sản phẩm đính kèm — mô tả bài viết phù hợp để đăng kèm hình.";

        return $"""
            Bạn là chuyên gia copywriting bán hàng Việt Nam. Hãy viết một bài quảng cáo Facebook/TikTok hấp dẫn.

            THÔNG TIN SẢN PHẨM:
            - Tên sản phẩm/dịch vụ: {productName}
            - Đặc điểm nổi bật:
            {features}
            - Ảnh sản phẩm: {imageNote}

            YÊU CẦU:
            - Giọng văn: {tone}
            - Công thức tâm lý: {formulaName}
            - Viết bằng tiếng Việt tự nhiên, có emoji phù hợp
            - Có hook mạnh ở dòng đầu, CTA rõ ràng ở cuối
            - Độ dài: 150-250 từ, tối ưu cho mạng xã hội
            - BẮT BUỘC nhắc lại các đặc điểm nổi bật đã liệt kê, không viết chung chung

            Chỉ trả về nội dung bài viết, không giải thích thêm.
            """;
    }

    public static string BuildDemoCopywritingPost(CopywritingRequest request)
    {
        var features = ParseFeatureLines(request.Features);
        var featureBullets = string.Join("\n", features.Select(f => $"• {f}"));
        var tone = request.Tone.Trim();
        var product = request.ProductName.Trim();

        if (request.Formula.Equals("PAS", StringComparison.OrdinalIgnoreCase))
            return BuildPasDemo(product, features, featureBullets, tone, request.ImageUrl);

        return BuildAidaDemo(product, features, featureBullets, tone, request.ImageUrl);
    }

    private static List<string> ParseFeatureLines(string features)
    {
        return features
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(f => f.Trim().TrimStart('-', '•', '*', ' '))
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .ToList();
    }

    private static string BuildAidaDemo(string product, List<string> features, string featureBullets, string tone, string? imageUrl)
    {
        var t = NormalizeTone(tone);

        var hook = t switch
        {
            _ when t.Contains("hai huoc") => $"😂 Mùi cơ thể \"báo động đỏ\"? {product} cứu cánh bạn đó!",
            _ when t.Contains("khan cap") || t.Contains("fomo") => $"⏰ ĐỪNG BỎ LỠ! {product} — deal hôm nay, mai hết slot!",
            _ when t.Contains("gan gui") => $"💬 Chị em ơi, mình tìm được {product} xịn lắm nè!",
            _ when t.Contains("chuyen nghiep") => $"✅ {product} — Giải pháp chăm sóc cá nhân chuẩn chất lượng.",
            _ when t.Contains("thuyet phuc") => $"🔥 {product} — Lựa chọn thông minh cho người biết chọn chất lượng!",
            _ => $"🔥 {product} — Bạn xứng đáng tự tin cả ngày dài!"
        };

        var interest = t switch
        {
            _ when t.Contains("chuyen nghiep") => "Được nhiều khách hàng tin dùng nhờ hiệu suất ổn định và an toàn cho da.",
            _ when t.Contains("hai huoc") => "Dùng xong đi gặp crush cũng không lo \"toang\" vì mùi nữa nha!",
            _ when t.Contains("gan gui") => "Mình dùng thử rồi — thật sự okela, recommend cho chị em liền!",
            _ => $"Mình dùng thử {product} và thấy khác biệt rõ rệt ngay từ lần đầu."
        };

        var desire = features.Count > 0
            ? $"Tại sao nên chọn ngay?\n{featureBullets}"
            : "Tại sao nên chọn ngay?\n• Chất lượng ổn định, phù hợp nhu cầu hàng ngày";

        var cta = t switch
        {
            _ when t.Contains("khan cap") || t.Contains("fomo") => "📩 INBOX \"MUA NGAY\" — số lượng có hạn trong hôm nay!",
            _ when t.Contains("chuyen nghiep") => "📞 Liên hệ ngay để được tư vấn và báo giá ưu đãi.",
            _ => "💬 Comment \"TƯ VẤN\" hoặc inbox shop để đặt hàng nhanh!"
        };

        return $"""
            {hook}

            {interest}

            💎 Điểm nổi bật:
            {desire}
            {(string.IsNullOrWhiteSpace(imageUrl) ? "" : "\n📸 Bài viết kèm ảnh sản phẩm — sẵn sàng đăng Facebook/TikTok.")}

            👉 {cta}

            #{Slugify(product)} #onesimply #banhang

            ---
            [Chế độ DEMO — dữ liệu lấy từ form của bạn. Điền DeepSeekKey/OpenAIKey trong appsettings.json để dùng AI thật.]
            """;
    }

    private static string BuildPasDemo(string product, List<string> features, string featureBullets, string tone, string? imageUrl)
    {
        var t = NormalizeTone(tone);

        var problem = t switch
        {
            _ when t.Contains("chuyen nghiep") => $"❌ Vấn đề: Mồ hôi và mùi cơ thể ảnh hưởng đến sự tự tin trong công việc hàng ngày.",
            _ when t.Contains("hai huoc") => $"😅 Ai cũng từng \"toang\" vì mùi cơ thể — kể cả lúc gặp khách quan trọng!",
            _ => $"❌ Bạn có đang lo lắng về mùi cơ thể khiến mình thiếu tự tin?"
        };

        var agitate = features.FirstOrDefault() is { Length: > 0 } firstFeature
            ? $"Càng trì hoãn, vấn đề càng khó chịu — đặc biệt khi bạn cần thoải mái cả ngày mà vẫn lo về \"{firstFeature.ToLower()}\"."
            : "Càng trì hoãn, bạn càng mất tự tin trong những khoảnh khắc quan trọng.";

        var solution = $"""
            ✅ Giải pháp: {product}
            {featureBullets}
            """;

        var cta = t.Contains("chuyen nghiep")
            ? "📩 Inbox để nhận tư vấn và đặt hàng chính hãng."
            : "🛒 Inbox ngay — ship nhanh, tư vấn nhiệt tình!";

        return $"""
            {problem}

            {agitate}

            {solution}
            {(string.IsNullOrWhiteSpace(imageUrl) ? "" : "\n📸 Bài viết kèm ảnh sản phẩm — sẵn sàng đăng Facebook/TikTok.")}

            👉 {cta}

            #{Slugify(product)} #onesimply #marketing

            ---
            [Chế độ DEMO — công thức PAS, giọng: {tone}. Điền API Key để dùng AI thật.]
            """;
    }

    private static string NormalizeTone(string tone)
    {
        var normalized = tone.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().ToLowerInvariant();
    }

    private static string Slugify(string text)
    {
        var slug = new string(text.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
        return string.Join("", slug.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(3));
    }
}
