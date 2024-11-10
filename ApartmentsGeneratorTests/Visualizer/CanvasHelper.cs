using NetTopologySuite.Geometries;
using SkiaSharp;

namespace ApartmentsGeneratorTests.Visualizer;

public static class CanvasHelper
{
    public static SKBitmap CreateBitmapWithBackground(Envelope bounds, float margin, float scale)
    {
        var canvasWidth = (float)(bounds.Width * scale) + margin * 2;
        var canvasHeight = (float)(bounds.Height * scale) + margin * 2;
        var bitmap = new SKBitmap((int)canvasWidth, (int)canvasHeight);

        using var canvas = new SKCanvas(bitmap);
        var backgroundPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 200),
            IsStroke = false,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRect(0, 0, canvasWidth, canvasHeight, backgroundPaint);

        return bitmap;
    }

    public static void ConfigureCanvasForFloor(SKCanvas canvas, Envelope bounds, float margin, float scale)
    {
        canvas.Translate(margin - (float)bounds.MinX * scale, margin - (float)bounds.MinY * scale);
        canvas.Scale(scale, scale);
    }

    public static void SaveBitmapAsPng(SKBitmap bitmap, string outputPath)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);
    }
}