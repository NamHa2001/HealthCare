using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace HealthCare.Infrastructure.Services.Ocr;

public class ImagePreprocessor
{
    public async Task<Stream> PreprocessAsync(Stream input, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(input, ct);

        image.Mutate(ctx => ctx
            .AutoOrient()
            .Grayscale()
            .GaussianSharpen(0.5f));

        var output = new MemoryStream();
        await image.SaveAsPngAsync(output, ct);
        output.Position = 0;
        return output;
    }
}
