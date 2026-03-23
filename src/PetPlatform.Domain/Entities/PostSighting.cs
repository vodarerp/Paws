using NetTopologySuite.Geometries;
using PetPlatform.Domain.Common;
using PetPlatform.Domain.ValueObjects;

namespace PetPlatform.Domain.Entities;

public class PostSighting : BaseEntity
{
    public Guid PostId { get; private set; }
    public Guid ReporterId { get; private set; }
    public Point Location { get; private set; } = default!;
    public string? LocationDescription { get; private set; }
    public DateTime SeenAt { get; private set; }
    public string? Comment { get; private set; }

    public Post Post { get; private set; } = default!;
    public User Reporter { get; private set; } = default!;

    public double Latitude => Location.Y;
    public double Longitude => Location.X;

    protected PostSighting() { }

    public static PostSighting Create(Guid postId, Guid reporterId,
        double latitude, double longitude, DateTime seenAt,
        string? locationDescription = null, string? comment = null)
    {
        return new PostSighting
        {
            PostId = postId,
            ReporterId = reporterId,
            Location = GeoPointFactory.Create(longitude, latitude),
            SeenAt = seenAt,
            LocationDescription = locationDescription?.Trim(),
            Comment = comment?.Trim()
        };
    }
}
