using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using UnitsNet;

namespace ApartmentsGenerator.Core
{
    public class FloorGenerator : IFloorGenerator
    {
        private const double HALLWAY_WIDTH_METERS = 4.5;
        private const double ELEVATOR_SHAFT_WIDTH_METERS = 6.6;
        private const int MODULES_PER_APARTMENT = 3;

        public Floor Generate(Polygon floorGeometry, Length cellWidthMeters, bool placeShaftOnTop = true)
        {
            var floor = new Floor(floorGeometry);
            var hallway = GenerateHallway(floorGeometry);
            AddFloorObjectWithCollisionCheck(floor, hallway);

            // Разделяем модули для верхнего и нижнего ряда
            var (topModules, bottomModules) = GenerateModules(floor, floorGeometry, cellWidthMeters);

            // Создаем шахту лифта по центру выбранной стороны, выбирая центральные модули
            var elevatorShaft = AssembleElevatorShaft(floor, topModules, bottomModules, placeShaftOnTop);
            AddFloorObjectWithCollisionCheck(floor, elevatorShaft);

            // Генерируем квартиры из оставшихся модулей
            var apartments = GenerateApartmentsFromModules(floor, bottomModules)
                             .Concat(GenerateApartmentsFromModules(floor, topModules))
                             .ToList();
            floor.FloorObjects.AddRange(apartments);

            return floor;
        }

        private static Hallway GenerateHallway(Polygon floorGeometry)
        {
            var availableArea = floorGeometry.EnvelopeInternal;
            var hallwayCoordinates = CalculateHallwayCoordinates(availableArea);
            var hallwayPolygon = new Polygon(new LinearRing(hallwayCoordinates));
            return new Hallway(new Floor(floorGeometry), hallwayPolygon);
        }

        private static Coordinate[] CalculateHallwayCoordinates(Envelope availableArea)
        {
            var hallwayWidth = Length.FromMeters(HALLWAY_WIDTH_METERS);
            var minX = Length.FromMeters(availableArea.MinX);
            var maxX = Length.FromMeters(availableArea.MaxX);
            var minY = Length.FromMeters(availableArea.MinY) + 
                       (Length.FromMeters(availableArea.Height) - hallwayWidth) / 2;
            var maxY = minY + hallwayWidth;

            return new[]
            {
                new Coordinate(minX.Meters, minY.Meters),
                new Coordinate(minX.Meters, maxY.Meters),
                new Coordinate(maxX.Meters, maxY.Meters),
                new Coordinate(maxX.Meters, minY.Meters),
                new Coordinate(minX.Meters, minY.Meters)
            };
        }

        private static (List<Polygon> bottomModules, List<Polygon> topModules) GenerateModules(Floor floor, Polygon floorGeometry, Length cellWidth)
        {
            var availableArea = floorGeometry.EnvelopeInternal;
            var moduleHeight = (Length.FromMeters(availableArea.Height) - Length.FromMeters(HALLWAY_WIDTH_METERS)) / 2;

            var startX = Length.FromMeters(availableArea.MinX);
            var bottomStartY = Length.FromMeters(availableArea.MinY);
            var topStartY = Length.FromMeters(availableArea.MaxY) - moduleHeight;

            var bottomModules = GenerateRowOfModules(startX, bottomStartY, availableArea, cellWidth, moduleHeight);
            var topModules = GenerateRowOfModules(startX, topStartY, availableArea, cellWidth, moduleHeight);

            return (bottomModules, topModules);
        }

        private static List<Polygon> GenerateRowOfModules(Length startX, Length startY, Envelope availableArea, Length cellWidth, Length moduleHeight)
        {
            var modules = new List<Polygon>();
            var currentX = startX;
            while (currentX + cellWidth <= Length.FromMeters(availableArea.MaxX))
            {
                modules.Add(CreateModulePolygon(currentX, startY, cellWidth, moduleHeight));
                currentX += cellWidth;
            }
            return modules;
        }

        private static Polygon CreateModulePolygon(Length startX, Length startY, Length width, Length height)
        {
            var endX = startX + width;
            var endY = startY + height;

            var coordinates = new[]
            {
                new Coordinate(startX.Meters, startY.Meters),
                new Coordinate(startX.Meters, endY.Meters),
                new Coordinate(endX.Meters, endY.Meters),
                new Coordinate(endX.Meters, startY.Meters),
                new Coordinate(startX.Meters, startY.Meters)
            };

            return new Polygon(new LinearRing(coordinates));
        }

        private static ElevatorShaft AssembleElevatorShaft(Floor floor, List<Polygon> topModules, List<Polygon> bottomModules, bool placeShaftOnTop)
        {
            var selectedModules = placeShaftOnTop ? topModules : bottomModules;

            // Выбираем центральные модули, необходимые для шахты, ориентируясь на нужную ширину
            var shaftModules = GetCentralModulesForShaft(selectedModules);

            // Объединяем выбранные модули в единый полигон для шахты
            var shaftPolygon = CombinePolygons(shaftModules);
            selectedModules.RemoveAll(m => shaftModules.Contains(m)); // Удаляем использованные модули

            return new ElevatorShaft(floor, shaftPolygon);
        }

        private static List<Polygon> GetCentralModulesForShaft(List<Polygon> modules)
        {
            // Определяем количество модулей, необходимых для покрытия ширины шахты
            int modulesNeeded = (int)Math.Ceiling(ELEVATOR_SHAFT_WIDTH_METERS / modules.First().EnvelopeInternal.Width);

            // Получаем центральные модули из списка
            int startIndex = (modules.Count - modulesNeeded) / 2;
            return modules.Skip(startIndex).Take(modulesNeeded).ToList();
        }

        private static List<Apartment> GenerateApartmentsFromModules(Floor floor, List<Polygon> modules)
        {
            var apartments = new List<Apartment>();

            // Группируем модули по 3, если остается меньше, используем их для последней квартиры
            while (modules.Count >= MODULES_PER_APARTMENT)
            {
                var apartmentModules = modules.Take(MODULES_PER_APARTMENT).ToList();
                var apartmentPolygon = CombinePolygons(apartmentModules);
                apartments.Add(new Apartment(floor, apartmentPolygon));
                modules.RemoveRange(0, MODULES_PER_APARTMENT); // Удаляем использованные модули
            }

            // Оставшиеся модули объединяем в одну квартиру, если есть
            if (modules.Any())
            {
                var remainingPolygon = CombinePolygons(modules);
                apartments.Add(new Apartment(floor, remainingPolygon));
            }

            return apartments;
        }

        private static Polygon CombinePolygons(List<Polygon> polygons)
        {
            var geometries = polygons.Cast<Geometry>().ToList();
            var union = CascadedPolygonUnion.Union(geometries);
            return (Polygon)union;
        }

        private static void AddFloorObjectWithCollisionCheck(Floor floor, FloorObject newObject)
        {
            foreach (var existingObject in floor.FloorObjects)
            {
                var intersection = existingObject.Bounds.Intersection(newObject.Bounds);
                
                // Проверка на непустое пересечение с ненулевой площадью
                if (intersection != null && intersection.Area > 0)
                {
                    throw new InvalidOperationException($"Объект {newObject.GetType().Name} пересекается с {existingObject.GetType().Name}");
                }
            }
            floor.FloorObjects.Add(newObject);
        }
    }
}
