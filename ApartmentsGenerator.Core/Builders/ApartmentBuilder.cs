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
            apartments.Add(new Apartment(floor, apartmentPolygon));
        }

        return apartments;
    }
}