using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using UnitsNet;

namespace ApartmentsGenerator.Core;

public static class GeometryHelper
{
    public static Polygon CombinePolygons(List<Polygon> polygons)
    {
        var geometries = polygons.Cast<Geometry>().ToList();
        var union = CascadedPolygonUnion.Union(geometries);

        // Проверяем, является ли результатом объединения Polygon или MultiPolygon
        return union as Polygon ?? (Polygon)union.ConvexHull();
    }

    public static Polygon CreateRectanglePolygon(Length startX, Length startY, Length width, Length height)
    {
        var endX = startX + width;
        var endY = startY + height;

        var coordinates = new[]
        {
            new Coordinate(startX.Meters, startY.Meters),
            new Coordinate(startX.Meters, endY.Meters),
            new Coordinate(endX.Meters, endY.Meters),
            new Coordinate(endX.Meters, startY.Meters),
            new Coordinate(startX.Meters, startY.Meters)
        };

        return new Polygon(new LinearRing(coordinates));
    }

    public static void AddFloorObjectWithCollisionCheck(Floor floor, FloorObject newObject)
    {
        foreach (var existingObject in floor.FloorObjects)
        {
            var intersection = existingObject.Bounds.Intersection(newObject.Bounds);
            if (intersection != null && intersection.Area > 0)
            {
                throw new InvalidOperationException($"Объект {newObject} пересекается с {existingObject}");
            }
        }
        floor.FloorObjects.Add(newObject);
    }
}