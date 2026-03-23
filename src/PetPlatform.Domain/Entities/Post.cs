using NetTopologySuite.Geometries;
using PetPlatform.Domain.Common;
using PetPlatform.Domain.Constants;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.Exceptions;
using PetPlatform.Domain.ValueObjects;

namespace PetPlatform.Domain.Entities;

public class Post : AuditableEntity
{
    public Guid AuthorId { get; private set; }
    public Guid? PetId { get; private set; }
    public PostCategory Category { get; private set; }
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string LocationZone { get; private set; } = default!;
    public Point? Location { get; private set; }
    public DateTime? LastSeenAt { get; private set; }
    public ContactPreference? ContactPreference { get; private set; }
    public int AlertRadiusKm { get; private set; } = 10;
    public DateTime? AlertSentAt { get; private set; }
    public PostStatus Status { get; private set; } = PostStatus.Active;
    public ResolutionType? ResolutionType { get; private set; }
    public int ReportCount { get; private set; }
    public bool IsHidden { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public User Author { get; private set; } = default!;
    public Pet? Pet { get; private set; }
    public ICollection<Media> Photos { get; private set; } = new List<Media>();
    public ICollection<PostSighting> Sightings { get; private set; } = new List<PostSighting>();
    public ICollection<Report> Reports { get; private set; } = new List<Report>();

    public double? Latitude => Location?.Y;
    public double? Longitude => Location?.X;

    protected Post() { }

    public static Post Create(Guid authorId, PostCategory category, string title,
        string description, string locationZone, Guid? petId = null,
        double? latitude = null, double? longitude = null,
        ContactPreference? contactPreference = null)
    {
        var post = new Post
        {
            AuthorId = authorId,
            PetId = petId,
            Category = category,
            Title = title.Trim(),
            Description = description.Trim(),
            LocationZone = locationZone.Trim(),
            Location = latitude.HasValue && longitude.HasValue
                ? GeoPointFactory.Create(longitude.Value, latitude.Value)
                : null,
            ContactPreference = contactPreference,
        };

        if (category == PostCategory.Lost)
        {
            post.ExpiresAt = DateTime.UtcNow.AddDays(RateLimits.LostPostExpirationDays);
            post.LastSeenAt = DateTime.UtcNow;
        }

        return post;
    }

    public void Update(string title, string description, string locationZone,
        double? latitude, double? longitude, ContactPreference? contactPreference)
    {
        Title = title.Trim();
        Description = description.Trim();
        LocationZone = locationZone.Trim();
        Location = latitude.HasValue && longitude.HasValue
            ? GeoPointFactory.Create(longitude.Value, latitude.Value)
            : null;
        ContactPreference = contactPreference;
        SetUpdated();
    }

    public void MarkAlertSent()
    {
        AlertSentAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void AddReport()
    {
        ReportCount++;
        // 3 prijave = auto-skrivanje
        if (ReportCount >= 3)
            IsHidden = true;
        SetUpdated();
    }

    public void Resolve(ResolutionType resolution)
    {
        if (Status == PostStatus.Closed)
            throw new PostAlreadyResolvedException();

        Status = PostStatus.Closed;
        ResolutionType = resolution;
        ExpiresAt = null;
        SetUpdated();
    }

    public void Extend()
    {
        if (Category != PostCategory.Lost)
            throw new DomainException("Samo objave za izgubljene ljubimce se mogu produžiti.", "INVALID_EXTEND");

        ExpiresAt = DateTime.UtcNow.AddDays(RateLimits.LostPostExpirationDays);
        SetUpdated();
    }

    public void Remove()
    {
        Status = PostStatus.Removed;
        SetUpdated();
    }
}
