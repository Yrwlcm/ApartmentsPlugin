using NetTopologySuite.Geometries;
using UnitsNet;

namespace ApartmentsGenerator.Core;

public static class ModuleGenerator
{
    public static (List<Polygon> topModules, List<Polygon> bottomModules) GenerateModules(Polygon floorGeometry,
        double hallwayWidthMeters,
        Length cellWidth)
    {
        var availableArea = floorGeometry.EnvelopeInternal;
        var moduleHeight = (Length.FromMeters(availableArea.Height) - Length.FromMeters(hallwayWidthMeters)) / 2;

        var startX = Length.FromMeters(availableArea.MinX);
        var bottomStartY = Length.FromMeters(availableArea.MinY);
        var topStartY = Length.FromMeters(availableArea.MaxY) - moduleHeight;

        var topModules = GenerateRowOfModules(startX, bottomStartY, availableArea, cellWidth, moduleHeight);
        var bottomModules = GenerateRowOfModules(startX, topStartY, availableArea, cellWidth, moduleHeight);

        return (topModules, bottomModules);
    }

    private static List<Polygon> GenerateRowOfModules(Length startX,
        Length startY,
        Envelope availableArea,
        Length cellWidth,
        Length moduleHeight)
    {
        var modules = new List<Polygon>();
        var currentX = startX;
        while (currentX + cellWidth <= Length.FromMeters(availableArea.MaxX))
        {
            modules.Add(GeometryHelper.CreateRectanglePolygon(currentX, startY, cellWidth, moduleHeight));
            currentX += cellWidth;
        }

        return modules;
    }
}