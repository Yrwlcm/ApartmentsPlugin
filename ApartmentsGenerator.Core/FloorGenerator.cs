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
            var hallway = GenerateHallway(floorGeometry);
            floor.FloorObjects.Add(hallway);

            var apartments = GenerateApartments(floor, floorGeometry, hallway.Bounds, cellWidthMeters);
            floor.FloorObjects.AddRange(apartments);
            return floor;
        }

        private static Hallway GenerateHallway(Polygon floorGeometry)
        {
            var availableArea = floorGeometry.EnvelopeInternal;
            var hallwayCoordinates = CalculateHallwayCoordinates(availableArea);
            var hallwayPolygon = new Polygon(new LinearRing(hallwayCoordinates));
            return new Hallway(new Floor(floorGeometry), hallwayPolygon);
        }

        private static Coordinate[] CalculateHallwayCoordinates(Envelope availableArea)
        {
            var hallwayWidth = Length.FromMeters(HALLWAY_WIDTH_METERS);
            var minX = Length.FromMeters(availableArea.MinX);
            var maxX = Length.FromMeters(availableArea.MaxX);
            var minY = Length.FromMeters(availableArea.MinY) + 
                       (Length.FromMeters(availableArea.Height) - hallwayWidth) / 2;
            var maxY = minY + hallwayWidth;

            return
            [
                new Coordinate(minX.Meters, minY.Meters),
                new Coordinate(minX.Meters, maxY.Meters),
                new Coordinate(maxX.Meters, maxY.Meters),
                new Coordinate(maxX.Meters, minY.Meters),
                new Coordinate(minX.Meters, minY.Meters)
            ];
        }

        private static List<Apartment> GenerateApartments(Floor floor,
            Polygon floorGeometry,
            Polygon hallwayGeometry,
            Length cellWidth)
        {
            var apartments = new List<Apartment>();
            var availableArea = floorGeometry.EnvelopeInternal;
            var apartmentHeight = CalculateApartmentHeight(availableArea, hallwayGeometry);

            apartments.AddRange(GenerateApartmentRow(floor, availableArea, cellWidth, apartmentHeight, true));
            apartments.AddRange(GenerateApartmentRow(floor, availableArea, cellWidth, apartmentHeight, false));

            return apartments;
        }

        private static Length CalculateApartmentHeight(Envelope availableArea, Polygon hallwayGeometry)
        {
            var hallwayHeight = Length.FromMeters(hallwayGeometry.EnvelopeInternal.Height);
            return (Length.FromMeters(availableArea.Height) - hallwayHeight) / 2;
        }

        private static IEnumerable<Apartment> GenerateApartmentRow(Floor floor,
            Envelope availableArea,
            Length cellWidth,
            Length apartmentHeight,
            bool isBottomRow)
        {
            var apartments = new List<Apartment>();
            var currentX = Length.FromMeters(availableArea.MinX);
            var startY = isBottomRow ? Length.FromMeters(availableArea.MinY) 
                                     : Length.FromMeters(availableArea.MaxY) - apartmentHeight;

            while (currentX + cellWidth <= Length.FromMeters(availableArea.MaxX))
            {
                apartments.Add(GenerateApartment(floor, currentX, startY, cellWidth, apartmentHeight));
                currentX += cellWidth;
            }

            return apartments;
        }

        private static Apartment GenerateApartment(Floor floor,
            Length startX,
            Length startY,
            Length width,
            Length height)
        {
            var coordinates = new[]
            {
                new Coordinate(startX.Meters, startY.Meters),
                new Coordinate(startX.Meters, (startY + height).Meters),
                new Coordinate((startX + width).Meters, (startY + height).Meters),
                new Coordinate((startX + width).Meters, startY.Meters),
                new Coordinate(startX.Meters, startY.Meters)
            };

            var apartmentPolygon = new Polygon(new LinearRing(coordinates));
            return new Apartment(floor, apartmentPolygon);
        }
    }
}
