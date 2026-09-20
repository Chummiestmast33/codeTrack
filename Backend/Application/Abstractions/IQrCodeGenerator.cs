namespace Backend.Application.Abstractions;

/// <summary>Renders QR payloads to PNG (RF-09). No System.Drawing: byte-based output.</summary>
public interface IQrCodeGenerator
{
    byte[] GeneratePng(string content, int targetSizePixels);
}
