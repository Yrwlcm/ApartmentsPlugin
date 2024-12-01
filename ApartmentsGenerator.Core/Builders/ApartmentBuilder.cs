using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core.Builders;

public static class ApartmentBuilder
{
    private const float DISTANCE_TOLERANCE_METERS = 0.01f;

    public static List<Apartment> GenerateApartmentsFromModules(Floor floor,
        List<Polygon> bottomModules, List<Polygon> topModules, int modulesPerApartment)
    {
        var apartments = new List<Apartment>();

        apartments.AddRange(GenerateApartmentsFromRow(floor, bottomModules, modulesPerApartment));
        apartments.AddRange(GenerateApartmentsFromRow(floor, topModules, modulesPerApartment));

        return apartments;
    }
    
    public static List<Apartment> GenerateApartmentsByArea(Floor floor,
        List<Polygon> topModules, List<Polygon> bottomModules, List<ApartmentType> apartmentTypes)
    {
        var totalArea = topModules.Concat(bottomModules).Sum(m => m.Area);
        var apartments = new List<Apartment>();
        var usedModules = new HashSet<Polygon>();

        foreach (var apartmentType in apartmentTypes)
        {
            var targetArea = apartmentType.Percentage / 100 * totalArea;
            var currentArea = 0.0;

            foreach (var batch in GenerateModuleBatches(topModules.Concat(bottomModules), usedModules, apartmentType))
            {
                var apartmentPolygon = GeometryHelper.CombinePolygons(batch);
                apartments.Add(new Apartment(floor, apartmentPolygon, apartmentType.Name));
                currentArea += apartmentPolygon.Area;

                if (currentArea >= targetArea)
                    break;
            }
        }

        return apartments;
    }

    private static List<Apartment> GenerateApartmentsFromRow(Floor floor, List<Polygon> modules, int modulesPerApartment)
    {
        var apartments = new List<Apartment>();

        for (var i = 0; i < modules.Count;)
        {
            var batch = TryGetAdjacentModules(modules, i, modulesPerApartment, out var nextIndex);

            if (batch.Count <= modulesPerApartment && batch.Count > 0)
            {
                var apartmentPolygon = GeometryHelper.CombinePolygons(batch);
                apartments.Add(new Apartment(floor, apartmentPolygon, ""));
            }

            i = nextIndex;
        }

        return apartments;
    }

    private static List<Polygon> TryGetAdjacentModules(List<Polygon> modules, int startIndex, int count, out int nextIndex)
    {
        var batch = new List<Polygon>();
        nextIndex = startIndex;

        for (var i = startIndex; i < modules.Count && batch.Count < count; i++)
        {
            if (batch.Count == 0 || ModulesAreAdjacent(batch.Last(), modules[i]))
            {
                batch.Add(modules[i]);
                nextIndex = i + 1;
            }
            else
            {
                break;
            }
        }

        return batch;
    }

    private static IEnumerable<List<Polygon>> GenerateModuleBatches(IEnumerable<Polygon> modules,
        HashSet<Polygon> usedModules, ApartmentType apartmentType)
    {
        var batch = new List<Polygon>();
        var batchArea = 0.0;

        foreach (var module in modules)
        {
            if (usedModules.Contains(module))
                continue;

            if (batch.Count != 0 && !ModulesAreAdjacent(batch.Last(), module))
            {
                batch.Clear();
                batchArea = 0.0;
            }

            batch.Add(module);
            batchArea += module.Area;

            if (batchArea >= apartmentType.MinArea && batchArea <= apartmentType.MaxArea)
            {
                foreach (var usedModule in batch)
                    usedModules.Add(usedModule);

                yield return [..batch];
                batch.Clear();
                batchArea = 0.0;
            }
        }
    }

    private static bool ModulesAreAdjacent(Polygon module1, Polygon module2)
    {
        var envelope1 = module1.EnvelopeInternal;
        var envelope2 = module2.EnvelopeInternal;

        return Math.Abs(envelope1.MaxX - envelope2.MinX) < DISTANCE_TOLERANCE_METERS ||
               Math.Abs(envelope1.MinX - envelope2.MaxX) < DISTANCE_TOLERANCE_METERS &&
               Math.Abs(envelope1.MaxY - envelope2.MaxY) < DISTANCE_TOLERANCE_METERS;
    }
}
