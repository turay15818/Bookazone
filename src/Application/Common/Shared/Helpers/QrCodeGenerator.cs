using QRCoder;

namespace Bookazone.Application.Common.Shared.Helpers;

public static class QrCodeGenerator
{
    public static string GenerateBase64(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);

        var bytes = qrCode.GetGraphic(20);
        return Convert.ToBase64String(bytes);
    }
}
