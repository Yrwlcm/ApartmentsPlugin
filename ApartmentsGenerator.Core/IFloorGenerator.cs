using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core;

public interface IFloorGenerator
{
    public Floor Generate(Polygon floorGeometry);
}