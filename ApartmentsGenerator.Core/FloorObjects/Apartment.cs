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
    
    public override void Accept(IFloorObjectVisitor visitor)
    {
        visitor.Visit(this);
    }
}