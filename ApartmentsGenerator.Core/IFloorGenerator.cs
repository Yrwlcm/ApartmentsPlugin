using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;
using UnitsNet;

namespace ApartmentsGenerator.Core;

public interface IFloorGenerator
{
    public Floor Generate(Polygon floorGeometry, Length cellWidthMeters);
}