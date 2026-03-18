# Domain Model Reference

## BaseEntity

```csharp
// Domain/Common/BaseEntity.cs
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(INotification domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void SetUpdated() => UpdatedAt = DateTime.UtcNow;
}

public abstract class AuditableEntity : BaseEntity
{
    public Guid CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }
}
```

---

## Enums

```csharp
public enum PostCategory
{
    Adoption,
    Lost,           // Aktivira Amber Alert push
    Found,
    FosterNeeded,   // Faza 1
    UrgentHelp,     // Faza 1
    Educational     // Faza 1
}

public enum PostStatus { Active, InProgress, Resolved, Expired, Removed }
public enum AlertStatus { Active, Updated, Resolved, Expired }
public enum UserRole { Individual, Foster, Organization, VetClinic, Admin, Moderator }

public enum TrustScoreCategory
{
    New,        // 0-19
    Active,     // 20-49
    Trusted,    // 50-99
    LocalHero   // 100+
}

public enum TrustScoreAction
{
    IdentityVerified,       // +20, jednokratno
    PostPublished,          // +2
    CommunityReaction,      // +2, cap +10/mesec
    LostAlertResolved,      // +5
    SuccessfulAdoption,     // +25, Faza 1
    FosterCompleted,        // +15, Faza 1
    ValidAbuseReport,       // +10, Faza 1
    ConfirmedReport,        // -30
    SlowResponse,           // -5, >48h bez odgovora
    UnresolvedExpiredAlert, // -10, Faza 1
    MaskedSale              // -50, Faza 1
}

public enum PetStatus { WithOwner, ForAdoption, InFoster, Lost }
public enum PetSize { Small, Medium, Large, ExtraLarge }
public enum PetGender { Male, Female, Unknown }
public enum NotificationCategory { AmberAlert, ChatMessage, StatusChange, TrustScore }
public enum ReportReason { MaskedSale, FakeAlert, Spam, InappropriateContent, Abuse }
public enum SanctionLevel { Warning, TemporarySuspension, PermanentBan }
```

---

## Value Objects (record struct — immutable)

```csharp
// Domain/ValueObjects/LocationZone.cs
public readonly record struct LocationZone(string Municipality, string? Settlement)
{
    public static LocationZone Create(string municipality, string? settlement = null)
    {
        if (string.IsNullOrWhiteSpace(municipality))
            throw new DomainException("Municipality cannot be empty.");
        return new LocationZone(municipality.Trim(), settlement?.Trim());
    }
    public override string ToString() =>
        Settlement != null ? $"{Settlement}, {Municipality}" : Municipality;
}

public readonly record struct GpsCoordinates(double Latitude, double Longitude)
{
    public static GpsCoordinates Create(double lat, double lng)
    {
        if (lat is < -90 or > 90) throw new DomainException("Invalid latitude.");
        if (lng is < -180 or > 180) throw new DomainException("Invalid longitude.");
        return new GpsCoordinates(lat, lng);
    }

    public double DistanceToMeters(GpsCoordinates other)
    {
        const double R = 6371000;
        var dLat = (other.Latitude - Latitude) * Math.PI / 180;
        var dLng = (other.Longitude - Longitude) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(Latitude * Math.PI / 180) * Math.Cos(other.Latitude * Math.PI / 180) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}

public readonly record struct TrustScorePoints(int Value)
{
    public static readonly TrustScorePoints Zero = new(0);
    public static TrustScorePoints Create(int value) => new(Math.Max(0, value));

    public TrustScoreCategory Category => Value switch
    {
        < 20 => TrustScoreCategory.New,
        < 50 => TrustScoreCategory.Active,
        < 100 => TrustScoreCategory.Trusted,
        _ => TrustScoreCategory.LocalHero
    };

    public TrustScorePoints Add(int points) => Create(Value + points);
    public TrustScorePoints Subtract(int points) => Create(Value - points);
}

public readonly record struct ImageHash(string Value)
{
    public static ImageHash Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Invalid image hash.");
        return new ImageHash(hash);
    }
}
```

---

## Entiteti

### User
```csharp
public class User : AuditableEntity
{
    public string Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string? AvatarUrl { get; private set; }
    public UserRole Role { get; private set; }
    public LocationZone LocationZone { get; private set; }
    public GpsCoordinates? LastKnownLocation { get; private set; }  // NIKAD se ne prikazuje
    public TrustScorePoints TrustScore { get; private set; } = TrustScorePoints.Zero;
    public bool IsVerified { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastActiveAt { get; private set; }
    public bool AmberAlertNotificationsEnabled { get; private set; } = true;
    public bool StatusChangeNotificationsEnabled { get; private set; } = true;

    public ICollection<Pet> Pets { get; private set; } = new List<Pet>();
    public ICollection<Post> Posts { get; private set; } = new List<Post>();

    protected User() { }

    public static User Create(string email, string displayName,
        LocationZone locationZone, UserRole role = UserRole.Individual)
    {
        var user = new User
        {
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            LocationZone = locationZone,
            Role = role
        };
        user.AddDomainEvent(new UserRegisteredEvent(user.Id));
        return user;
    }

    public void ApplyTrustScoreChange(TrustScoreAction action)
    {
        var before = TrustScore;
        var points = TrustScoreCalculator.GetPoints(action);
        TrustScore = points > 0 ? TrustScore.Add(points) : TrustScore.Subtract(Math.Abs(points));
        SetUpdated();
        if (before.Category != TrustScore.Category)
            AddDomainEvent(new TrustScoreCategoryChangedEvent(Id, before.Category, TrustScore.Category));
    }

    public void UpdateLastKnownLocation(GpsCoordinates coordinates)
    {
        LastKnownLocation = coordinates;
        SetUpdated();
    }

    public void Verify()
    {
        if (IsVerified) return;
        IsVerified = true;
        ApplyTrustScoreChange(TrustScoreAction.IdentityVerified);
    }
}
```

### Post
```csharp
public class Post : AuditableEntity
{
    public Guid AuthorId { get; private set; }
    public Guid? PetId { get; private set; }
    public PostCategory Category { get; private set; }
    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public IReadOnlyList<string> MediaUrls { get; private set; } = new List<string>();
    public ImageHash? PrimaryImageHash { get; private set; }
    public LocationZone LocationZone { get; private set; }
    public GpsCoordinates? IncidentLocation { get; private set; }
    public bool IsUrgent { get; private set; }
    public PostStatus Status { get; private set; } = PostStatus.Active;
    public DateTime? ExpiresAt { get; private set; }
    public int ReportCount { get; private set; }
    public bool IsHidden { get; private set; }

    public User Author { get; private set; } = default!;
    public Pet? Pet { get; private set; }
    public ICollection<Report> Reports { get; private set; } = new List<Report>();
    public ICollection<PetSighting> Sightings { get; private set; } = new List<PetSighting>();

    protected Post() { }

    public static Post Create(Guid authorId, PostCategory category, string title,
        string content, List<string> mediaUrls, LocationZone zone,
        Guid? petId = null, GpsCoordinates? incidentLocation = null)
    {
        if (!mediaUrls.Any())
            throw new DomainException("Post must have at least one photo.", "POST_NO_PHOTO");

        var post = new Post
        {
            AuthorId = authorId, PetId = petId, Category = category,
            Title = title.Trim(), Content = content.Trim(),
            MediaUrls = mediaUrls.AsReadOnly(), LocationZone = zone,
            IncidentLocation = incidentLocation,
            IsUrgent = category is PostCategory.Lost or PostCategory.UrgentHelp,
        };

        if (category == PostCategory.Lost)
            post.ExpiresAt = DateTime.UtcNow.AddDays(7);

        post.AddDomainEvent(new PostCreatedEvent(post.Id, authorId, category));

        if (category == PostCategory.Lost)
            post.AddDomainEvent(new AmberAlertActivatedEvent(
                post.Id, authorId, incidentLocation, zone));

        return post;
    }

    public void AddReport(Guid reporterId, ReportReason reason)
    {
        ReportCount++;
        if (ReportCount >= 3)
        {
            IsHidden = true;
            AddDomainEvent(new PostAutoHiddenEvent(Id, ReportCount));
        }
    }

    public void Resolve()
    {
        if (Status == PostStatus.Resolved)
            throw new PostAlreadyResolvedException();
        Status = PostStatus.Resolved;
        ExpiresAt = null;
        SetUpdated();
        AddDomainEvent(new PostResolvedEvent(Id, AuthorId, Category));
    }

    public void Extend()
    {
        if (Category != PostCategory.Lost)
            throw new DomainException("Only Lost posts can be extended.");
        ExpiresAt = DateTime.UtcNow.AddDays(7);
        SetUpdated();
    }
}
```

### Pet
```csharp
public class Pet : AuditableEntity
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Breed { get; private set; } = default!;
    public int? AgeMonths { get; private set; }
    public PetGender Gender { get; private set; }
    public PetSize Size { get; private set; }
    public string? ChipNumber { get; private set; }
    public bool IsSterilized { get; private set; }
    public string? Color { get; private set; }
    public string? DistinctiveMarks { get; private set; }
    public IReadOnlyList<string> PhotoUrls { get; private set; } = new List<string>();
    public PetStatus Status { get; private set; } = PetStatus.WithOwner;
    // AI polja (Faza 1.5)
    public string? AiDetectedBreed { get; private set; }
    public double? AiConfidence { get; private set; }

    public User Owner { get; private set; } = default!;

    protected Pet() { }

    public static Pet Create(Guid ownerId, string name, string breed,
        PetGender gender, PetSize size, List<string> photoUrls,
        int? ageMonths = null, string? color = null)
    {
        if (!photoUrls.Any())
            throw new DomainException("Pet must have at least one photo.");
        return new Pet
        {
            OwnerId = ownerId, Name = name.Trim(), Breed = breed.Trim(),
            Gender = gender, Size = size, AgeMonths = ageMonths,
            Color = color?.Trim(), PhotoUrls = photoUrls.AsReadOnly()
        };
    }

    public void MarkAsLost() { Status = PetStatus.Lost; SetUpdated(); }
    public void MarkAsFound() { Status = PetStatus.WithOwner; SetUpdated(); }
    public void MarkForAdoption() { Status = PetStatus.ForAdoption; SetUpdated(); }
}
```

### PetSighting ("Vidim ovog psa")
```csharp
public class PetSighting : BaseEntity
{
    public Guid PostId { get; private set; }
    public Guid ReporterId { get; private set; }
    public GpsCoordinates Location { get; private set; }
    public DateTime SeenAt { get; private set; }
    public string? Comment { get; private set; }

    protected PetSighting() { }

    public static PetSighting Create(Guid postId, Guid reporterId,
        GpsCoordinates location, string? comment = null)
    {
        var s = new PetSighting
        {
            PostId = postId, ReporterId = reporterId,
            Location = location, SeenAt = DateTime.UtcNow,
            Comment = comment?.Trim()
        };
        s.AddDomainEvent(new PetSightingReportedEvent(postId, reporterId, location));
        return s;
    }
}
```

### Chat
```csharp
public class ChatConversation : BaseEntity
{
    public Guid PostId { get; private set; }
    public Guid InitiatorId { get; private set; }
    public Guid PostOwnerId { get; private set; }
    public bool PhoneShared { get; private set; } = false;
    public DateTime? LastMessageAt { get; private set; }
    public ICollection<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();

    protected ChatConversation() { }

    public static ChatConversation Create(Guid postId, Guid initiatorId, Guid postOwnerId)
        => new() { PostId = postId, InitiatorId = initiatorId, PostOwnerId = postOwnerId };

    public void SharePhone(Guid userId)
    {
        if (userId != InitiatorId && userId != PostOwnerId)
            throw new DomainException("Only conversation participants can share phone.");
        PhoneShared = true;
        SetUpdated();
        AddDomainEvent(new PhoneSharedInConversationEvent(Id, userId));
    }
}

public class ChatMessage : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = default!;
    public bool IsRead { get; private set; }

    protected ChatMessage() { }

    public static ChatMessage Create(Guid conversationId, Guid senderId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Message content cannot be empty.");
        var msg = new ChatMessage
        {
            ConversationId = conversationId, SenderId = senderId, Content = content.Trim()
        };
        msg.AddDomainEvent(new ChatMessageSentEvent(conversationId, senderId));
        return msg;
    }

    public void MarkAsRead() { IsRead = true; SetUpdated(); }
}
```

### TrustScoreHistory + Report + Notification
```csharp
public class TrustScoreHistory : BaseEntity
{
    public Guid UserId { get; private set; }
    public TrustScoreAction Action { get; private set; }
    public int Points { get; private set; }
    public int BalanceBefore { get; private set; }
    public int BalanceAfter { get; private set; }
    public string? Description { get; private set; }
    public Guid? ReferenceId { get; private set; }

    protected TrustScoreHistory() { }

    public static TrustScoreHistory Create(Guid userId, TrustScoreAction action,
        int points, int before, int after, Guid? referenceId = null)
        => new()
        {
            UserId = userId, Action = action, Points = points,
            BalanceBefore = before, BalanceAfter = after, ReferenceId = referenceId,
            Description = TrustScoreCalculator.GetActionDescription(action)
        };
}

public class Report : BaseEntity
{
    public Guid ReporterId { get; private set; }
    public string TargetType { get; private set; } = default!;  // "Post", "User"
    public Guid TargetId { get; private set; }
    public ReportReason Reason { get; private set; }
    public string? Evidence { get; private set; }
    public string Status { get; private set; } = "Pending";
    public string? AdminNotes { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    protected Report() { }

    public static Report Create(Guid reporterId, string targetType,
        Guid targetId, ReportReason reason, string? evidence = null)
        => new()
        {
            ReporterId = reporterId, TargetType = targetType,
            TargetId = targetId, Reason = reason, Evidence = evidence
        };
}

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public string Title { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public bool IsRead { get; private set; }
    public bool IsPush { get; private set; }

    protected Notification() { }

    public static Notification Create(Guid userId, NotificationCategory category,
        string title, string body, Guid? referenceId = null,
        string? referenceType = null, bool isPush = false)
        => new()
        {
            UserId = userId, Category = category, Title = title, Body = body,
            ReferenceId = referenceId, ReferenceType = referenceType, IsPush = isPush
        };

    public void MarkAsRead() { IsRead = true; SetUpdated(); }
}
```

---

## Domain Events (MediatR INotification)

```csharp
public record UserRegisteredEvent(Guid UserId) : INotification;
public record TrustScoreCategoryChangedEvent(
    Guid UserId, TrustScoreCategory Before, TrustScoreCategory After) : INotification;

public record PostCreatedEvent(
    Guid PostId, Guid AuthorId, PostCategory Category) : INotification;
public record PostResolvedEvent(
    Guid PostId, Guid AuthorId, PostCategory Category) : INotification;
public record PostAutoHiddenEvent(Guid PostId, int ReportCount) : INotification;

public record AmberAlertActivatedEvent(
    Guid PostId, Guid ReporterId,
    GpsCoordinates? Location, LocationZone Zone) : INotification;

public record PetSightingReportedEvent(
    Guid PostId, Guid ReporterId, GpsCoordinates Location) : INotification;

public record ChatMessageSentEvent(
    Guid ConversationId, Guid SenderId) : INotification;
public record PhoneSharedInConversationEvent(
    Guid ConversationId, Guid SharedBy) : INotification;
```

---

## Domain Exceptions

```csharp
public class DomainException : Exception
{
    public string Code { get; }
    public DomainException(string message, string? code = null)
        : base(message) { Code = code ?? "DOMAIN_ERROR"; }
}

public class PostLimitExceededException()
    : DomainException("Maximum 3 posts per day.", "POST_LIMIT_EXCEEDED");

public class AlertCooldownException()
    : DomainException("Only 1 Lost alert per 24 hours.", "ALERT_COOLDOWN");

public class UnauthorizedPostActionException()
    : DomainException("You are not the author of this post.", "UNAUTHORIZED_POST_ACTION");

public class PostAlreadyResolvedException()
    : DomainException("This post is already resolved.", "POST_ALREADY_RESOLVED");
```

---

## Servis Interfejsi

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IUserRepository : IRepository<User>
{
    Task<IEnumerable<Guid>> GetUsersInRadiusAsync(
        GpsCoordinates center, double radiusMeters, CancellationToken ct = default);
    Task<IEnumerable<Guid>> GetUsersByZoneAsync(
        LocationZone zone, CancellationToken ct = default);
}

public interface IPostRepository : IRepository<Post>
{
    Task<int> GetDailyPostCountAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasActiveLostPostAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Post>> GetFeedAsync(Guid userId, LocationZone zone,
        PostCategory? filter, int page, CancellationToken ct = default);
}

public interface INotificationService
{
    Task SendPushAsync(Guid userId, string title, string body,
        Dictionary<string, string>? data = null, CancellationToken ct = default);
    Task SendPushToManyAsync(IEnumerable<Guid> userIds, string title, string body,
        Dictionary<string, string>? data = null, CancellationToken ct = default);
}

public interface IImageHashingService
{
    Task<ImageHash> ComputeHashAsync(Stream imageStream, CancellationToken ct = default);
    Task<bool> IsDuplicateAsync(ImageHash hash, CancellationToken ct = default);
}

public interface IFcmTokenRepository
{
    Task<IEnumerable<string>> GetTokensForUserAsync(Guid userId, CancellationToken ct = default);
    Task UpsertTokenAsync(Guid userId, string token, CancellationToken ct = default);
}
```

---

## TrustScoreCalculator (Domain Service)

```csharp
public static class TrustScoreCalculator
{
    private static readonly Dictionary<TrustScoreAction, int> _points = new()
    {
        { TrustScoreAction.IdentityVerified,       +20 },
        { TrustScoreAction.PostPublished,          +2  },
        { TrustScoreAction.CommunityReaction,      +2  },
        { TrustScoreAction.LostAlertResolved,      +5  },
        { TrustScoreAction.SuccessfulAdoption,     +25 },
        { TrustScoreAction.FosterCompleted,        +15 },
        { TrustScoreAction.ValidAbuseReport,       +10 },
        { TrustScoreAction.ConfirmedReport,        -30 },
        { TrustScoreAction.SlowResponse,           -5  },
        { TrustScoreAction.UnresolvedExpiredAlert, -10 },
        { TrustScoreAction.MaskedSale,             -50 }
    };

    public static int GetPoints(TrustScoreAction action) =>
        _points.TryGetValue(action, out var p) ? p : 0;

    public static string GetActionDescription(TrustScoreAction action) => action switch
    {
        TrustScoreAction.IdentityVerified   => "Verifikovan identitet",
        TrustScoreAction.PostPublished      => "Objavljen oglas",
        TrustScoreAction.LostAlertResolved  => "Pas nađen — oglas zatvoren",
        TrustScoreAction.SuccessfulAdoption => "Uspešno udomljavanje",
        TrustScoreAction.ConfirmedReport    => "Potvrđena prijava od zajednice",
        TrustScoreAction.SlowResponse       => "Neodgovaranje na poruke >48h",
        _ => action.ToString()
    };
}
```
