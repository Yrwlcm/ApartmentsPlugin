using NetTopologySuite.Geometries;
using SkiaSharp;

namespace ApartmentsGeneratorTests.Visualizer;

public static class PolygonDrawer
{
    public static void DrawPolygon(Polygon polygon, SKCanvas canvas, SKPaint paint)
    {
        var coordinates = polygon.Coordinates;
        for (var i = 0; i < coordinates.Length - 1; i++)
        {
            var start = new SKPoint((float)coordinates[i].X, (float)coordinates[i].Y);
            var end = new SKPoint((float)coordinates[i + 1].X, (float)coordinates[i + 1].Y);
            canvas.DrawLine(start, end, paint);
        }
    }

    public static void DrawFilledPolygon(Polygon polygon, SKCanvas canvas, SKPaint paint)
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