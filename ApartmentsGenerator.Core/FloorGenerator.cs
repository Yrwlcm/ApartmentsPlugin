using ApartmentsGenerator.Core;
using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;
using UnitsNet;

public class FloorGenerator : IFloorGenerator
{
    private const double HALLWAY_WIDTH_METERS = 4.5;
    private const double ELEVATOR_SHAFT_WIDTH_METERS = 6.6;
    private const int MODULES_PER_APARTMENT = 3;

    public Floor Generate(Polygon floorGeometry, Length cellWidthMeters, bool placeShaftOnTop = true)
    {
        var floor = new Floor(floorGeometry);

        // Генерация коридора
        var hallway = HallwayBuilder.GenerateHallway(floorGeometry, HALLWAY_WIDTH_METERS);
        GeometryHelper.AddFloorObjectWithCollisionCheck(floor, hallway);

        // Генерация модулей
        var (topModules, bottomModules) =
            ModuleGenerator.GenerateModules(floorGeometry, HALLWAY_WIDTH_METERS, cellWidthMeters);

        // Сборка лифтовой шахты
        var elevatorShaft = ElevatorShaftBuilder.AssembleElevatorShaft(floor, topModules, bottomModules,
            placeShaftOnTop, ELEVATOR_SHAFT_WIDTH_METERS);
        GeometryHelper.AddFloorObjectWithCollisionCheck(floor, elevatorShaft);

        // Создание квартир
        var apartments =
            ApartmentBuilder.GenerateApartmentsFromModules(floor, bottomModules, topModules, MODULES_PER_APARTMENT);
        floor.FloorObjects.AddRange(apartments);

        return floor;
    }
}