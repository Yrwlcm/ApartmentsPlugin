using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core.FloorObjects;

public class ElevatorShaft: FloorObject
{
    public override FloorObjectType FloorObjectType => FloorObjectType.ElevatorShaft;

    public Floor Floor { get; set; }

    public ElevatorShaft(Floor floor, Polygon bounds)
    {
        Floor = floor;
        Bounds = bounds;
    }
    
    public override void Accept(IFloorObjectVisitor visitor)
    {
        visitor.Visit(this);
    }
}