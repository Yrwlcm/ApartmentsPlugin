using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;

namespace ApartmentsGenerator.Core
{
    public class FloorGenerator : IFloorGenerator
    {
        public Floor Generate(Polygon floorGeometry)
        {
            var floor = new Floor(floorGeometry);
            var apartments = GenerateApartments(floor);
            floor.FloorObjects.AddRange(apartments);
            return floor;
        }

        private static List<Apartment> GenerateApartments(Floor floor)
        {
            var apartments = new List<Apartment>();
            var availableArea = floor.Bounds;
            
            var totalWidth = availableArea.EnvelopeInternal.Width;
            var apartmentWidth = totalWidth / 10;
            var apartmentHeight = availableArea.EnvelopeInternal.Height;

            var currentX = availableArea.EnvelopeInternal.MinX;
            var currentY = availableArea.EnvelopeInternal.MinY;

            for (var i = 0; i < 10; i++)
            {
                var apartmentCoordinates = new[]
                {
                    new Coordinate(currentX, currentY),
                    new Coordinate(currentX, currentY + apartmentHeight),
                    new Coordinate(currentX + apartmentWidth, currentY + apartmentHeight),
                    new Coordinate(currentX + apartmentWidth, currentY),
                    new Coordinate(currentX, currentY)
                };

                var apartmentPolygon = new Polygon(new LinearRing(apartmentCoordinates));
                var apartment = new Apartment(floor, apartmentPolygon);
                apartments.Add(apartment);

                currentX += apartmentWidth;
            }

            return apartments;
        }
    }
}