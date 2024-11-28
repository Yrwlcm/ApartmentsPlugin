using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core.FloorObjects;

public abstract class FloorObject
{
    public abstract FloorObjectType FloorObjectType { get; }
    public Polygon Bounds { get; protected set; } 
    
    public override string ToString()
    {
        var center = Bounds.Centroid;
        var width = Bounds.EnvelopeInternal.Width;
        var height = Bounds.EnvelopeInternal.Height;
        return $"Type: {FloorObjectType}, Center: ({center.X}, {center.Y}), Width: {width}, Height: {height}";
    }
    
    public abstract void Accept(IFloorObjectVisitor visitor);
}