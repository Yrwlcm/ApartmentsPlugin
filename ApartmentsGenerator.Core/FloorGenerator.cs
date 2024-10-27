using ApartmentsGenerator.Core.FloorObjects;
using NetTopologySuite.Geometries;
using UnitsNet;

namespace ApartmentsGenerator.Core
{
    public class FloorGenerator : IFloorGenerator
    {
        private const double HALLWAY_WIDTH_METERS = 4.5;
        
        public Floor Generate(Polygon floorGeometry, Length cellWidthMeters)
        {
            var floor = new Floor(floorGeometry);
            var hallway = GenerateHallway(floor);
            floor.FloorObjects.Add(hallway);

            var apartments = GenerateApartments(floor, hallway.Bounds, cellWidthMeters);
            floor.FloorObjects.AddRange(apartments);
            return floor;
        }

        private static Hallway GenerateHallway(Floor floor)
        {
            var availableArea = floor.Bounds;
            var hallwayWidth = Length.FromMeters(HALLWAY_WIDTH_METERS);
            var hallwayMinX = Length.FromMeters(availableArea.EnvelopeInternal.MinX);
            var hallwayMaxX = Length.FromMeters(availableArea.EnvelopeInternal.MaxX);
            var hallwayMinY = Length.FromMeters(availableArea.EnvelopeInternal.MinY) 
                              + (Length.FromMeters(availableArea.EnvelopeInternal.Height) - hallwayWidth) / 2;
            var hallwayMaxY = hallwayMinY + hallwayWidth;

            var hallwayCoordinates = new[]
            {
                new Coordinate(hallwayMinX.Meters, hallwayMinY.Meters),
                new Coordinate(hallwayMinX.Meters, hallwayMaxY.Meters),
                new Coordinate(hallwayMaxX.Meters, hallwayMaxY.Meters),
                new Coordinate(hallwayMaxX.Meters, hallwayMinY.Meters),
                new Coordinate(hallwayMinX.Meters, hallwayMinY.Meters)
            };

            var hallwayPolygon = new Polygon(new LinearRing(hallwayCoordinates));
            return new Hallway(floor, hallwayPolygon);
        }

        private static List<Apartment> GenerateApartments(Floor floor, Polygon hallwayGeometry, Length cellWidth)
        {
            var apartments = new List<Apartment>();
            var availableArea = floor.Bounds;
            var hallwayWidth = Length.FromMeters(hallwayGeometry.EnvelopeInternal.Height);
            var apartmentHeight = (Length.FromMeters(availableArea.EnvelopeInternal.Height) - hallwayWidth) / 2;

            var currentX = Length.FromMeters(availableArea.EnvelopeInternal.MinX);
            var currentYBottom = Length.FromMeters(availableArea.EnvelopeInternal.MinY);
            var currentYTop = Length.FromMeters(availableArea.EnvelopeInternal.MaxY) - apartmentHeight;

            // Generate apartments below the hallway
            while (currentX + cellWidth <= Length.FromMeters(availableArea.EnvelopeInternal.MaxX))
            {
                var bottomApartmentCoordinates = new[]
                {
                    new Coordinate(currentX.Meters, currentYBottom.Meters),
                    new Coordinate(currentX.Meters, (currentYBottom + apartmentHeight).Meters),
                    new Coordinate((currentX + cellWidth).Meters, (currentYBottom + apartmentHeight).Meters),
                    new Coordinate((currentX + cellWidth).Meters, currentYBottom.Meters),
                    new Coordinate(currentX.Meters, currentYBottom.Meters)
                };

                var bottomApartmentPolygon = new Polygon(new LinearRing(bottomApartmentCoordinates));
                var bottomApartment = new Apartment(floor, bottomApartmentPolygon);
                apartments.Add(bottomApartment);

                // Generate apartments above the hallway
                var topApartmentCoordinates = new[]
                {
                    new Coordinate(currentX.Meters, currentYTop.Meters),
                    new Coordinate(currentX.Meters, (currentYTop + apartmentHeight).Meters),
                    new Coordinate((currentX + cellWidth).Meters, (currentYTop + apartmentHeight).Meters),
                    new Coordinate((currentX + cellWidth).Meters, currentYTop.Meters),
                    new Coordinate(currentX.Meters, currentYTop.Meters)
                };

                var topApartmentPolygon = new Polygon(new LinearRing(topApartmentCoordinates));
                var topApartment = new Apartment(floor, topApartmentPolygon);
                apartments.Add(topApartment);

                currentX += cellWidth;
            }

            return apartments;
        }
    }
}
