using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core.FloorObjects;

public class Apartment : FloorObject
{
    public override FloorObjectType FloorObjectType => FloorObjectType.Apartment;
    public Floor Floor { get; set; }

    public Apartment(Floor floor, Polygon bounds)
    {
        Floor = floor;
        Bounds = bounds;
    }
}