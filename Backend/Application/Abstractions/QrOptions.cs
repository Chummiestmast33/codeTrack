namespace Backend.Application.Abstractions;

/// <summary>QR attendance settings (RF-08, RF-10).</summary>
public sealed class QrOptions
{
    public const string SectionName = "Qr";

    public int ExpiryMinutes { get; set; } = 120;

    public int ImageSizePixels { get; set; } = 512;

    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}
