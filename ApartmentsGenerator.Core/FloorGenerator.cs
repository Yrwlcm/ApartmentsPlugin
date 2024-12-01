using ApartmentsGenerator.Core;
using ApartmentsGenerator.Core.Builders;
using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;
using UnitsNet;

public class FloorGenerator : IFloorGenerator
{
    private const float HALLWAY_WIDTH_METERS = 4.5f;
    private const float ELEVATOR_SHAFT_WIDTH_METERS = 6.6f;
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

    public Floor Generate(Polygon floorGeometry, List<ApartmentType> apartmentTypes, bool placeShaftOnTop = true)
    {
        if (apartmentTypes == null || apartmentTypes.Count == 0)
            throw new ArgumentException("Необходимо указать типы квартир.");

        if (Math.Abs(apartmentTypes.Sum(at => at.Percentage) - 100) > 0.001)
            throw new ArgumentException("Сумма процентов всех типов квартир должна быть равна 100%.");

        var optimalWidth = CalculateModuleWidthBasedOnAreas(floorGeometry, 3.3, apartmentTypes, HALLWAY_WIDTH_METERS);

        var floor = new Floor(floorGeometry);

        // Генерация коридора
        var hallway = HallwayBuilder.GenerateHallway(floorGeometry, HALLWAY_WIDTH_METERS);
        GeometryHelper.AddFloorObjectWithCollisionCheck(floor, hallway);

        // Генерация модулей
        var (topModules, bottomModules) =
            ModuleGenerator.GenerateModules(floorGeometry, HALLWAY_WIDTH_METERS, optimalWidth);

        // Сборка лифтовой шахты
        var elevatorShaft = ElevatorShaftBuilder.AssembleElevatorShaft(floor, topModules, bottomModules,
            placeShaftOnTop, ELEVATOR_SHAFT_WIDTH_METERS);
        GeometryHelper.AddFloorObjectWithCollisionCheck(floor, elevatorShaft);

        var apartments = ApartmentBuilder.GenerateApartmentsByArea(floor, topModules, bottomModules, apartmentTypes);
        floor.FloorObjects.AddRange(apartments);

        return floor;
    }


    public static Length CalculateModuleWidthBasedOnAreas(Polygon floorGeometry, double minModuleWidthMeters,
        List<ApartmentType> apartmentTypes, double hallwayWidthMeters)
    {
        var buildingWidth = floorGeometry.EnvelopeInternal.Width;
        var buildingHeight = floorGeometry.EnvelopeInternal.Height;

        var moduleLength = (buildingHeight - hallwayWidthMeters) / 2;

        // Вычисление минимальной площади однокомнатной квартиры
        var minAreaPerRoom = apartmentTypes
            .Where(at => at.Rooms == 1)
            .Select(at => at.MinArea)
            .DefaultIfEmpty(apartmentTypes.Min(at => at.MinArea / at.Rooms))
            .Min();

        // Подбор минимальной ширины модуля
        for (var width = Math.Max(minModuleWidthMeters, minAreaPerRoom / moduleLength);
             width <= buildingWidth;
             width += 0.1)
        {
            var moduleArea = moduleLength * width;

            // Проверяем, можно ли достичь минимальной площади однокомнатной квартиры
            if (moduleArea < minAreaPerRoom)
                continue;

            // Проверяем, можно ли достичь диапазонов площадей для всех квартир
            var isValid = apartmentTypes.All(at =>
            {
                var minRooms = Math.Ceiling(at.MinArea / moduleArea);
                var maxRooms = Math.Floor(at.MaxArea / moduleArea);
                return minRooms <= at.Rooms && maxRooms >= at.Rooms;
            });

            if (isValid)
                return Length.FromMeters(width);
        }

        throw new InvalidOperationException(
            "Не удалось подобрать ширину модуля, удовлетворяющую всем диапазонам площадей.");
    }
}