using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core;

public static class ApartmentBuilder
{
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

        while (modules.Count >= modulesPerApartment)
        {
            var apartmentModules = modules.Take(modulesPerApartment).ToList();
            var apartmentPolygon = GeometryHelper.CombinePolygons(apartmentModules);
            apartments.Add(new Apartment(floor, apartmentPolygon));
            modules.RemoveRange(0, modulesPerApartment);
        }

        if (modules.Count == 0)
            return apartments;

        var remainingPolygon = GeometryHelper.CombinePolygons(modules);
        apartments.Add(new Apartment(floor, remainingPolygon));

        return apartments;
    }
}