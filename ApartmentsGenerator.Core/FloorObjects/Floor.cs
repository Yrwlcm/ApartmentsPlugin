using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core.FloorObjects;

public class Floor : FloorObject
{
    public override FloorObjectType FloorObjectType => FloorObjectType.Floor;
    
    public List<FloorObject> FloorObjects { get; set; } = [];

    public Floor(Polygon floorPolygon)
    {
        Bounds = floorPolygon;
    }
    
    public override void Accept(IFloorObjectVisitor visitor)
    {
        visitor.Visit(this);
    }
}