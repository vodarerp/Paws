using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Domain.ValueObjects;

public readonly record struct Location(double Latitude, double Longitude)
{
    public static Location Create(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            throw new DomainException("Nevalidna geografska širina.", "INVALID_LATITUDE");
        if (longitude is < -180 or > 180)
            throw new DomainException("Nevalidna geografska dužina.", "INVALID_LONGITUDE");

        return new Location(latitude, longitude);
    }
}
