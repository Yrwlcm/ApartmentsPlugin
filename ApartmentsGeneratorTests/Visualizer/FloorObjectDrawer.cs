using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace ApartmentsGeneratorTests.Visualizer;

public class FloorObjectDrawer(SKCanvas canvas) : IFloorObjectVisitor
{
    public void Visit(Floor floor)
    {
        var floorOutlinePaint = new SKPaint
        {
            Color = SKColors.DarkGray,
            IsStroke = true,
            StrokeWidth = 2, // Более толстый контур для выделения границы этажа
            Style = SKPaintStyle.Stroke
        };
        PolygonDrawer.DrawPolygon(floor.Bounds, canvas, floorOutlinePaint);
    }
    
    public void Visit(Apartment apartment)
    {
        var fillPaint = new SKPaint
        {
            Color = SKColors.LightGreen,
            IsStroke = false,
            Style = SKPaintStyle.Fill
        };
        PolygonDrawer.DrawFilledPolygon(apartment.Bounds, canvas, fillPaint);

        var outlinePaint = new SKPaint
        {
            Color = SKColors.Black,
            IsStroke = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke
        };
        PolygonDrawer.DrawPolygon(apartment.Bounds, canvas, outlinePaint);
    }

    public void Visit(Hallway hallway)
    {
        var fillPaint = new SKPaint
        {
            Color = SKColors.LightBlue,
            IsStroke = false,
            Style = SKPaintStyle.Fill
        };
        PolygonDrawer.DrawFilledPolygon(hallway.Bounds, canvas, fillPaint);

        var outlinePaint = new SKPaint
        {
            Color = SKColors.Blue,
            IsStroke = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke
        };
        PolygonDrawer.DrawPolygon(hallway.Bounds, canvas, outlinePaint);
    }

    public void Visit(ElevatorShaft elevatorShaft)
    {
        var fillPaint = new SKPaint
        {
            Color = SKColors.RosyBrown,
            IsStroke = false,
            Style = SKPaintStyle.Fill
        };
        PolygonDrawer.DrawFilledPolygon(elevatorShaft.Bounds, canvas, fillPaint);

        var outlinePaint = new SKPaint
        {
            Color = SKColors.Black,
            IsStroke = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke
        };
        PolygonDrawer.DrawPolygon(elevatorShaft.Bounds, canvas, outlinePaint);
    }

    public void Visit(FloorObject floorObject)
    {
        // Общий метод для FloorObject, если объект не является Apartment, Hallway или ElevatorShaft
        var fillPaint = new SKPaint
        {
            Color = SKColors.Crimson,
            IsStroke = false,
            Style = SKPaintStyle.Fill
        };
        PolygonDrawer.DrawFilledPolygon(floorObject.Bounds, canvas, fillPaint);

        var outlinePaint = new SKPaint
        {
            Color = SKColors.Black,
            IsStroke = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke
        };
        PolygonDrawer.DrawPolygon(floorObject.Bounds, canvas, outlinePaint);
    }
}