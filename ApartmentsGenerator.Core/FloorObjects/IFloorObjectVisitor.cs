namespace ApartmentsGenerator.Core.FloorObjects;

public interface IFloorObjectVisitor
{
    void Visit(Apartment apartment);
    void Visit(Hallway hallway);
    void Visit(ElevatorShaft elevatorShaft);
    void Visit(Floor floor);
}