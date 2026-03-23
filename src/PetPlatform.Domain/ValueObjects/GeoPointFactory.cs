using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace PetPlatform.Domain.ValueObjects;

public static class GeoPointFactory
{
    private static readonly GeometryFactory Factory = NtsGeometryServices.Instance
        .CreateGeometryFactory(srid: 4326);

    public static Point Create(double longitude, double latitude)
        => Factory.CreatePoint(new Coordinate(longitude, latitude));
}
