using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core;

public static class ElevatorShaftBuilder
{
    public static ElevatorShaft AssembleElevatorShaft(Floor floor,
        List<Polygon> topModules,
        List<Polygon> bottomModules,
        bool placeShaftOnTop,
        double shaftWidthMeters)
    {
        var selectedModules = placeShaftOnTop ? topModules : bottomModules;
        var shaftModules = GetCentralModulesForShaft(selectedModules, shaftWidthMeters);

        var shaftPolygon = GeometryHelper.CombinePolygons(shaftModules);
        selectedModules.RemoveAll(m => shaftModules.Contains(m));

        return new ElevatorShaft(floor, shaftPolygon);
    }

    private static List<Polygon> GetCentralModulesForShaft(List<Polygon> modules, double shaftWidthMeters)
    {
        var modulesNeeded = (int)Math.Ceiling(shaftWidthMeters / modules.First().EnvelopeInternal.Width);
        var startIndex = (modules.Count - modulesNeeded) / 2;
        return modules.Skip(startIndex).Take(modulesNeeded).ToList();
    }
}