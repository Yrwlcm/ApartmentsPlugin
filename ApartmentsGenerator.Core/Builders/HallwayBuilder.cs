using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;
using UnitsNet;

namespace ApartmentsGenerator.Core.Builders;

public static class HallwayBuilder
{
    public static Hallway GenerateHallway(Polygon floorGeometry, double hallwayWidth)
    {
        var availableArea = floorGeometry.EnvelopeInternal;
        var hallwayCoordinates = CalculateHallwayCoordinates(availableArea, hallwayWidth);
        var hallwayPolygon = new Polygon(new LinearRing(hallwayCoordinates));
        return new Hallway(new Floor(floorGeometry), hallwayPolygon);
    }

    private static Coordinate[] CalculateHallwayCoordinates(Envelope availableArea, double hallwayWidthMeters)
    {
        var hallwayWidth = Length.FromMeters(hallwayWidthMeters);
        var minX = Length.FromMeters(availableArea.MinX);
        var maxX = Length.FromMeters(availableArea.MaxX);
        var minY = Length.FromMeters(availableArea.MinY) +
                   (Length.FromMeters(availableArea.Height) - hallwayWidth) / 2;
        var maxY = minY + hallwayWidth;

        return
        [
            new Coordinate(minX.Meters, minY.Meters),
            new Coordinate(minX.Meters, maxY.Meters),
            new Coordinate(maxX.Meters, maxY.Meters),
            new Coordinate(maxX.Meters, minY.Meters),
            new Coordinate(minX.Meters, minY.Meters)
        ];
    }
}