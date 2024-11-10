using ApartmentsGenerator.Core.FloorObjects;
using ApartmentsGeneratorTests.Visualizer;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace ApartmentsGeneratorTests;

public static class FloorVisualizer
{
    public static void GenerateFloorVisualization(Floor floor, string outputPath)
    {
        const float margin = 20f;
        const float scale = 4f;
        var bounds = floor.Bounds.EnvelopeInternal;

        using var bitmap = CanvasHelper.CreateBitmapWithBackground(bounds, margin, scale);
        using var canvas = new SKCanvas(bitmap);

        CanvasHelper.ConfigureCanvasForFloor(canvas, bounds, margin, scale);

        var drawer = new FloorObjectDrawer(canvas);
        floor.Accept(drawer);
        foreach (var floorObject in floor.FloorObjects)
        {
            floorObject.Accept(drawer);
        }

        CanvasHelper.SaveBitmapAsPng(bitmap, outputPath);
    }
}