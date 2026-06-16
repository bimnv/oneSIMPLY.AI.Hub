namespace oneSIMPLY.AIHub.Api.Services;

public static class PaymentHelper
{
    public static bool TryParseTransactionCode(string code, out int userId, out int subscriptionId)
    {
        userId = 0;
        subscriptionId = 0;

        if (!code.StartsWith("OSAI", StringComparison.OrdinalIgnoreCase))
            return false;

        var tIndex = code.LastIndexOf('T');
        if (tIndex <= 4)
            return false;

        var sIndex = code.LastIndexOf('S', tIndex - 1);
        if (sIndex <= 4)
            return false;

        return int.TryParse(code.Substring(4, sIndex - 4), out userId)
            && int.TryParse(code.Substring(sIndex + 1, tIndex - sIndex - 1), out subscriptionId);
    }
}
