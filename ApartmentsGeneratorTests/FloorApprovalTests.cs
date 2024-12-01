using ApartmentsGenerator.Core;
using ApartmentsGenerator.Core.FloorObjects;
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

    [Test]
    public async Task GenerateAndVerifyFloorVisualization_WithPercentageGeneration()
    {
        const int FLOOR_WIDTH_METERS = 100;
        const int FLOOR_HEIGHT_METERS = 50;

        // Определение типов квартир
        var apartmentTypes = new List<ApartmentType>
        {
            new("Люкс", rooms: 3, minArea: 152, maxArea: 301, percentage: 20),
            new("Комфорт", rooms: 2, minArea: 102, maxArea: 151, percentage: 50),
            new("Эконом", rooms: 1, minArea: 50, maxArea: 101, percentage: 30)
        };

        var geometryFactory = new GeometryFactory();
        var coordinates = new[]
        {
            new Coordinate(0, 0),
            new Coordinate(0, FLOOR_HEIGHT_METERS),
            new Coordinate(FLOOR_WIDTH_METERS, FLOOR_HEIGHT_METERS),
            new Coordinate(FLOOR_WIDTH_METERS, 0),
            new Coordinate(0, 0)
        };

        var polygon = geometryFactory.CreatePolygon(coordinates);
        var floorGenerator = new FloorGenerator();
        var floor = floorGenerator.Generate(polygon, apartmentTypes);

        // Вычисляем общую площадь этажа
        var totalFloorArea = floor.Bounds.Area;

        // Подсчитываем данные о сгенерированных квартирах
        foreach (var apartmentType in apartmentTypes)
        {
            var apartments = floor.FloorObjects
                .OfType<Apartment>()
                .Where(a => a.Name == apartmentType.Name)
                .ToList();

            var totalApartmentArea = apartments.Sum(a => a.Bounds.Area);
            var actualPercentage = (totalApartmentArea / totalFloorArea) * 100;

            Console.WriteLine($"Тип квартир: {apartmentType.Name}");
            Console.WriteLine($"  Количество квартир: {apartments.Count}");
            Console.WriteLine($"  Занятая площадь: {totalApartmentArea:F2} м²");
            Console.WriteLine($"  Фактический процент площади: {actualPercentage:F2}%");
        }

        // Генерация визуализации
        var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory,
            "FloorVisualization_NamedApartments.png");
        FloorVisualizer.GenerateFloorVisualization(floor, outputPath);

        await VerifyFile(outputPath);
    }
}