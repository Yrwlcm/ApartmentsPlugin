using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace ApartmentsGeneratorTests;

public static class FloorVisualizer
{
    public static void GenerateFloorVisualization(Floor floor, string outputPath)
    {
        var bounds = floor.Bounds.EnvelopeInternal;
        const float margin = 20f;
        const float scale = 4f; // Масштабирование для улучшенного отображения

        var canvasWidth = (float)(bounds.Width * scale) + margin * 2;
        var canvasHeight = (float)(bounds.Height * scale) + margin * 2;

        using var bitmap = new SKBitmap((int)canvasWidth, (int)canvasHeight);
        using var canvas = new SKCanvas(bitmap);
        // Устанавливаем серый фон для всего изображения
        var backgroundPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 200),
            IsStroke = false,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRect(0, 0, canvasWidth, canvasHeight, backgroundPaint);

        var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsStroke = true,
            StrokeWidth = 2
        };

        // Рисуем этаж и объекты с учетом смещения для центрирования
        canvas.Translate(margin - (float)bounds.MinX * scale, margin - (float)bounds.MinY * scale);
        canvas.Scale(scale, scale);

        // Рисуем этаж
        DrawPolygon(floor.Bounds, canvas, paint);

        // Рисуем квартиры
        foreach (var floorObject in floor.FloorObjects)
        {
            if (floorObject is Apartment apartment)
            {
                DrawPolygon(apartment.Bounds, canvas, paint);
            }
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);
    }

    private static void DrawPolygon(Polygon polygon, SKCanvas canvas, SKPaint paint)
    {
        var coordinates = polygon.Coordinates;
        for (var i = 0; i < coordinates.Length - 1; i++)
        {
            var start = new SKPoint((float)coordinates[i].X, (float)coordinates[i].Y);
            var end = new SKPoint((float)coordinates[i + 1].X, (float)coordinates[i + 1].Y);
            canvas.DrawLine(start, end, paint);
        }
    }
}