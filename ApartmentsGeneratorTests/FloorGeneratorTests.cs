using NetTopologySuite.Geometries;
using ApartmentsGenerator.Core;
using FluentAssertions;

namespace ApartmentsGeneratorTests;

public class FloorGeneratorShould
{
    [Test]
    public void GenerateFloor_WithCorrectBounds()
    {
        // Создаем фабрику геометрий для создания объекта Polygon
        var geometryFactory = new GeometryFactory();

        // Определяем координаты прямоугольника
        var coordinates = new[]
        {
            new Coordinate(0, 0),   // Нижний левый угол
            new Coordinate(0, 10),  // Верхний левый угол
            new Coordinate(20, 10), // Верхний правый угол
            new Coordinate(20, 0),  // Нижний правый угол
            new Coordinate(0, 0)    // Замыкающая точка для формирования контура
        };

        // Создаем полигон, используя координаты
        var rectanglePolygon = geometryFactory.CreatePolygon(coordinates);
        
        var floorGenerator = new FloorGenerator();
        var floor = floorGenerator.Generate(rectanglePolygon);
        floor.Bounds.Should().Be(rectanglePolygon);
    }
}