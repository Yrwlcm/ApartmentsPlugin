using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core.FloorObjects;

public class Apartment : FloorObject
{
    public override FloorObjectType FloorObjectType => FloorObjectType.Apartment;
    public Floor Floor { get; set; }
    public string Name { get; set; } // Новое свойство для имени

    public Apartment(Floor floor, Polygon bounds, string name)
    {
        Floor = floor;
        Bounds = bounds;
        Name = name;
    }

    public override void Accept(IFloorObjectVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return base.ToString() + $", Name: {Name}";
    }
}