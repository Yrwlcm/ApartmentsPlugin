namespace ApartmentsGenerator.Core;

public readonly struct ApartmentType
{
    public string Name { get; }
    public int Rooms { get; }
    public double MinArea { get; }
    public double MaxArea { get; }
    public double Percentage { get; }

    public ApartmentType(string name, int rooms, double minArea, double maxArea, double percentage)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя квартиры не может быть пустым.", nameof(name));
        if (rooms <= 0)
            throw new ArgumentOutOfRangeException(nameof(rooms), "Количество комнат должно быть больше 0.");
        if (minArea <= 0 || maxArea <= 0 || minArea > maxArea)
            throw new ArgumentOutOfRangeException(nameof(minArea), "Площади указаны неверно.");
        if (percentage is <= 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Процент должен быть в диапазоне (0, 100].");

        Name = name;
        Rooms = rooms;
        MinArea = minArea;
        MaxArea = maxArea;
        Percentage = percentage;
    }

    public override string ToString()
    {
        return $"{Name} ({Rooms} комнаты, {MinArea:F2}-{MaxArea:F2} м², {Percentage:F2}%)";
    }
}
