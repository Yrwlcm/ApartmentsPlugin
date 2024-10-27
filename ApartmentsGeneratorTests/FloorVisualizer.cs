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

        // Рисуем этаж и объекты с учетом смещения для центрирования
        canvas.Translate(margin - (float)bounds.MinX * scale, margin - (float)bounds.MinY * scale);
        canvas.Scale(scale, scale);

        // Рисуем этаж
        var floorPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsStroke = true,
            StrokeWidth = 1, // Потоньше, чтобы не перекрывать другие элементы
            Style = SKPaintStyle.Stroke
        };
        DrawPolygon(floor.Bounds, canvas, floorPaint);

        // Рисуем объекты этажа (квартиры и коридоры)
        foreach (var floorObject in floor.FloorObjects)
        {
            if (floorObject is Apartment apartment)
            {
                var apartmentPaint = new SKPaint
                {
                    Color = SKColors.LightGreen,
                    IsStroke = false,
                    Style = SKPaintStyle.Fill
                };
                DrawFilledPolygon(apartment.Bounds, canvas, apartmentPaint);

                var apartmentOutlinePaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsStroke = true,
                    StrokeWidth = 1,
                    Style = SKPaintStyle.Stroke
                };
                DrawPolygon(apartment.Bounds, canvas, apartmentOutlinePaint);
            }
            else if (floorObject is Hallway hallway)
            {
                var hallwayPaint = new SKPaint
                {
                    Color = SKColors.LightBlue,
                    IsStroke = false,
                    Style = SKPaintStyle.Fill
                };
                DrawFilledPolygon(hallway.Bounds, canvas, hallwayPaint);

                var hallwayOutlinePaint = new SKPaint
                {
                    Color = SKColors.Blue,
                    IsStroke = true,
                    StrokeWidth = 1,
                    Style = SKPaintStyle.Stroke
                };
                DrawPolygon(hallway.Bounds, canvas, hallwayOutlinePaint);
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

    private static void DrawFilledPolygon(Polygon polygon, SKCanvas canvas, SKPaint paint)
    {
        using var path = new SKPath();
        var coordinates = polygon.Coordinates;
        if (coordinates.Length <= 0) 
            return;
            
        path.MoveTo((float)coordinates[0].X, (float)coordinates[0].Y);
        for (var i = 1; i < coordinates.Length; i++)
        {
            path.LineTo((float)coordinates[i].X, (float)coordinates[i].Y);
        }
        path.Close();
        canvas.DrawPath(path, paint);
    }
}