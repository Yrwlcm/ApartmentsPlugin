using ApartmentsGenerator.Core;
using NetTopologySuite.Geometries;
using UnitsNet;
using UnitsNet.Units;

namespace ApartmentsGeneratorTests;

[TestFixture]
public class FloorApprovalTests
{
    [Test]
    public async Task GenerateAndVerifyFloorVisualization()
    {
        const int FLOOR_WIDTH_METERS = 100;
        const int FLOOR_HEIGHT_METERS = 50;
        const double SEGMENT_WIDTH_METERS = 3.3;
        
        var geometryFactory = new GeometryFactory();
        var coordinates = new[]
        {
            new Coordinate(0, 0),
            new Coordinate(0, FLOOR_HEIGHT_METERS),
            new Coordinate(FLOOR_WIDTH_METERS, FLOOR_HEIGHT_METERS),
            new Coordinate(FLOOR_WIDTH_METERS, 0),
            new Coordinate(0, 0)
        };
        var segmentLength = new Length(SEGMENT_WIDTH_METERS, LengthUnit.Meter);
        
        var polygon = geometryFactory.CreatePolygon(coordinates);
        var floorGenerator = new FloorGenerator();
        var floor = floorGenerator.Generate(polygon, segmentLength);

        var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "FloorVisualization.png");
        FloorVisualizer.GenerateFloorVisualization(floor, outputPath);
        
        await VerifyFile(outputPath);
    }
}