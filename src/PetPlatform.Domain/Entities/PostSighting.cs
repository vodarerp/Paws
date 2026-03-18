using PetPlatform.Domain.Common;

namespace PetPlatform.Domain.Entities;

public class PostSighting : BaseEntity
{
    public Guid PostId { get; private set; }
    public Guid ReporterId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string? LocationDescription { get; private set; }
    public DateTime SeenAt { get; private set; }
    public string? Comment { get; private set; }

    public Post Post { get; private set; } = default!;
    public User Reporter { get; private set; } = default!;

    protected PostSighting() { }

    public static PostSighting Create(Guid postId, Guid reporterId,
        double latitude, double longitude, DateTime seenAt,
        string? locationDescription = null, string? comment = null)
    {
        return new PostSighting
        {
            PostId = postId,
            ReporterId = reporterId,
            Latitude = latitude,
            Longitude = longitude,
            SeenAt = seenAt,
            LocationDescription = locationDescription?.Trim(),
            Comment = comment?.Trim()
        };
    }
}
