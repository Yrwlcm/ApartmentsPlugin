using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core.FloorObjects;

public abstract class FloorObject
{
    public abstract FloorObjectType FloorObjectType { get; }
    public Polygon Bounds { get; protected set; } 
}

public enum FloorObjectType
{
    Floor = 0,
    Apartment = 1,
}