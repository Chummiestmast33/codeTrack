using Backend.Application.Abstractions;
using QRCoder;

namespace Backend.Infrastructure.Qr;

/// <summary>Byte-based QR rendering (RF-09); no System.Drawing, container-safe.</summary>
public sealed class QrCodeGenerator : IQrCodeGenerator
{
    public byte[] GeneratePng(string content, int targetSizePixels)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        if (targetSizePixels <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetSizePixels));
        }

        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var pixelsPerModule = Math.Max(1, targetSizePixels / data.ModuleMatrix.Count);
        return new PngByteQRCode(data).GetGraphic(pixelsPerModule);
    }
}
