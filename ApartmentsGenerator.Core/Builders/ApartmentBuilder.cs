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
        List<Polygon> availableModules, List<ApartmentType> apartmentTypes)
    {
        var apartments = new List<Apartment>();

        // Разделяем модули на верхние и нижние
        var topModules = availableModules.Where(m => m.Centroid.Y > floor.Bounds.Centroid.Y).ToList();
        var bottomModules = availableModules.Where(m => m.Centroid.Y <= floor.Bounds.Centroid.Y).ToList();

        foreach (var apartmentType in apartmentTypes)
        {
            var targetArea = apartmentType.Percentage / 100 * availableModules.Sum(m => m.Area);
            var currentArea = 0.0;

            while (currentArea < targetArea && (topModules.Count != 0 || bottomModules.Count != 0))
            {
                var modulesToCombine = new List<Polygon>();
                var combinedArea = 0.0;

                // Выбираем, откуда брать модули (сверху или снизу)
                var moduleRow = topModules.Count != 0 ? topModules : bottomModules;

                foreach (var module in moduleRow.ToList())
                {
                    if (combinedArea + module.Area > apartmentType.MaxArea)
                        break;

                    modulesToCombine.Add(module);
                    combinedArea += module.Area;
                    moduleRow.Remove(module);

                    // Проверка, можно ли завершить текущую квартиру
                    if (combinedArea >= apartmentType.MinArea && combinedArea <= apartmentType.MaxArea)
                    {
                        var apartmentPolygon = GeometryHelper.CombinePolygons(modulesToCombine);
                        apartments.Add(new Apartment(floor, apartmentPolygon, apartmentType.Name));
                        currentArea += combinedArea;
                        break;
                    }
                }

                // Если не удалось создать квартиру, выбрасываем исключение
                if (combinedArea < apartmentType.MinArea)
                {
                    throw new InvalidOperationException(
                        $"Не удалось создать квартиру типа {apartmentType.Name} в заданных пределах площади.");
                }
            }
        }

        return apartments;
    }
}