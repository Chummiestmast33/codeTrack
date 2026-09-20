using Backend.Infrastructure.Qr;

namespace Backend.Tests.Features.Catalog;

public sealed class QrCodeGeneratorTests
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    [Fact]
    public void GeneratePng_Returns_Valid_Png()
    {
        var generator = new QrCodeGenerator();

        var png = generator.GeneratePng("http://localhost:5173/app/attend/abc123", 512);

        Assert.True(png.Length > 100);
        Assert.Equal(PngSignature, png[..8]);
    }

    [Fact]
    public void GeneratePng_Rejects_Empty_Content_And_Size()
    {
        var generator = new QrCodeGenerator();
        Assert.Throws<ArgumentException>(() => generator.GeneratePng("  ", 512));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.GeneratePng("x", 0));
    }
}
