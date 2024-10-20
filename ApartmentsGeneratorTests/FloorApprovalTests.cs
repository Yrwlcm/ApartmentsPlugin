using ApartmentsGenerator.Core;
using NetTopologySuite.Geometries;

namespace ApartmentsGeneratorTests;

[TestFixture]
public class FloorApprovalTests
{
    [Test]
    public async Task GenerateAndVerifyFloorVisualization()
    {
        // Генерируем изображение текущего результата
        var geometryFactory = new GeometryFactory();
        var coordinates = new[]
        {
            new Coordinate(0, 0),
            new Coordinate(0, 100),
            new Coordinate(300, 100),
            new Coordinate(300, 0),
            new Coordinate(0, 0)
        };
        var polygon = geometryFactory.CreatePolygon(coordinates);
        var floorGenerator = new FloorGenerator();
        var floor = floorGenerator.Generate(polygon);

        var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "FloorVisualization.png");
        FloorVisualizer.GenerateFloorVisualization(floor, outputPath);
        
        await VerifyFile(outputPath);
    }
}