using ApartmentsGenerator.Core.FloorObjects;

namespace ApartmentsGenerator.Core;

public class Building(List<Floor> floors)
{
    public List<Floor> Floors { get; set; } = floors;
}