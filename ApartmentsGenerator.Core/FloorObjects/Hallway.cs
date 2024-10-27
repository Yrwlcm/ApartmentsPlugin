using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core.FloorObjects;

public class Hallway: FloorObject
{
    public override FloorObjectType FloorObjectType => FloorObjectType.Hallway;
    public Floor Floor { get; set; }

    public Hallway(Floor floor, Polygon bounds)
    {
        Floor = floor;
        Bounds = bounds;
    }
}