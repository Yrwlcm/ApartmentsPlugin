using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core.Builders;

public static class ApartmentBuilder
{
    private const float DISTANCE_TOLERANCE_METERS = 0.01f;

    public static List<Apartment> GenerateApartmentsFromModules(Floor floor,
        List<Polygon> bottomModules,
        List<Polygon> topModules,
        int modulesPerApartment)
    {
        var apartments = new List<Apartment>();

        apartments.AddRange(CreateApartmentsFromModules(floor, bottomModules, modulesPerApartment));
        apartments.AddRange(CreateApartmentsFromModules(floor, topModules, modulesPerApartment));

        return apartments;
    }

    private static List<Apartment> CreateApartmentsFromModules(Floor floor,
        List<Polygon> modules,
        int modulesPerApartment)
    {
        var apartments = new List<Apartment>();

        while (modules.Count > 0)
        {
            var modulesToCombine = new List<Polygon>();

            while (modulesToCombine.Count < modulesPerApartment && modules.Count > 0)
            {
                var previousModule = modulesToCombine.LastOrDefault();
                var nextModule = modules.FirstOrDefault();

                if (nextModule is null)
                    break;

                if (previousModule is null || nextModule.Distance(previousModule) < DISTANCE_TOLERANCE_METERS)
                {
                    modulesToCombine.Add(nextModule);
                    modules.RemoveAt(0);
                }
                else
                    break;
            }

            var apartmentPolygon = GeometryHelper.CombinePolygons(modulesToCombine);
            apartments.Add(new Apartment(floor, apartmentPolygon, ""));
        }

        return apartments;
    }

    public static List<Apartment> GenerateApartmentsByArea(Floor floor,
        List<Polygon> topModules, List<Polygon> bottomModules, List<ApartmentType> apartmentTypes)
    {
        var totalArea = topModules.Concat(bottomModules).Sum(m => m.Area);
        var apartments = new List<Apartment>();

        foreach (var apartmentType in apartmentTypes)
        {
            var targetArea = apartmentType.Percentage / 100 * totalArea;
            var currentArea = 0.0;

            var currentRow = topModules.Count > 0 ? topModules : bottomModules;

            while (currentArea < targetArea)
            {
                if (currentRow.Count == 0)
                {
                    // Если текущий ряд пуст, переключаемся на другой
                    currentRow = currentRow == topModules ? bottomModules : topModules;

                    // Если другой ряд тоже пуст, завершаем цикл
                    if (currentRow.Count == 0)
                        break;
                }

                var modulesToCombine = new List<Polygon>();
                var combinedArea = 0.0;
                var apartmentCreated = false;

                for (var i = 0; i < currentRow.Count; i++)
                {
                    var module = currentRow[i];
                    
                    if (modulesToCombine.Count != 0)
                    {
                        var lastModule = modulesToCombine.Last();
                        if (!ModulesAreAdjacent(lastModule, module))
                        {
                            modulesToCombine.Clear();
                            combinedArea = 0.0;
                            continue;
                        }
                    }
                    modulesToCombine.Add(module);
                    combinedArea += module.Area;

                    // Если квартира сформирована в пределах площади
                    if (combinedArea >= apartmentType.MinArea && combinedArea <= apartmentType.MaxArea)
                    {
                        var apartmentPolygon = GeometryHelper.CombinePolygons(modulesToCombine);
                        apartments.Add(new Apartment(floor, apartmentPolygon, apartmentType.Name));
                        currentArea += combinedArea;
                        modulesToCombine.ForEach(m => currentRow.Remove(m));

                        apartmentCreated = true;
                        break;
                    }
                }

                // Если не удалось создать квартиру
                if (!apartmentCreated)
                {
                    // Переходим на другой ряд, если невозможно продолжить в текущем
                    if (currentRow == topModules && bottomModules.Count >= apartmentType.Rooms)
                    {
                        currentRow = bottomModules;
                        continue;
                    }

                    break;
                }
            }
        }

        return apartments;
    }
    
    private static bool ModulesAreAdjacent(Polygon module1, Polygon module2)
    {
        var envelope1 = module1.EnvelopeInternal;
        var envelope2 = module2.EnvelopeInternal;

        // Проверяем, что модули соприкасаются по горизонтали и находятся на 1 вертикали
        return Math.Abs(envelope1.MaxX - envelope2.MinX) < 0.01 || Math.Abs(envelope1.MinX - envelope2.MaxX) < 0.01 &&
            Math.Abs(envelope1.MaxY - envelope2.MaxY) < 0.01;
    }
}