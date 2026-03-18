# CQRS Konvencije i Kod Standardi

## NuGet Paketi po Sloju

### Domain
```
(nema eksternih zavisnosti — čist C#)
```

### Application
```
MediatR
FluentValidation
FluentValidation.DependencyInjectionExtensions
Microsoft.Extensions.Logging.Abstractions
```

### Infrastructure
```
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.AspNetCore.Authentication.JwtBearer
Hangfire.AspNetCore
Hangfire.SqlServer
Azure.Storage.Blobs
FirebaseAdmin
SixLabors.ImageSharp         # Image hashing
```

### API
```
Microsoft.AspNetCore.SignalR
Microsoft.Azure.SignalR
Swashbuckle.AspNetCore
Serilog.AspNetCore
Microsoft.ApplicationInsights.AspNetCore
```

---

## CQRS Pattern

### Command struktura

```csharp
// Application/Commands/Posts/CreatePost/CreatePostCommand.cs
public record CreatePostCommand(
    Guid AuthorId,
    string Category,
    string Title,
    string Content,
    List<string> MediaUrls,
    string Municipality,
    string? Settlement,
    double? IncidentLatitude,
    double? IncidentLongitude,
    Guid? PetId
) : IRequest<CreatePostResponse>;

// Application/Commands/Posts/CreatePost/CreatePostResponse.cs
public record CreatePostResponse(
    Guid PostId,
    string Status,
    bool AmberAlertTriggered,
    DateTime? ExpiresAt
);

// Application/Commands/Posts/CreatePost/CreatePostHandler.cs
public class CreatePostHandler : IRequestHandler<CreatePostCommand, CreatePostResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPublisher _publisher;

    public CreatePostHandler(IPostRepository postRepository,
        IUserRepository userRepository, IPublisher publisher)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _publisher = publisher;
    }

    public async Task<CreatePostResponse> Handle(
        CreatePostCommand request, CancellationToken ct)
    {
        var dailyCount = await _postRepository.GetDailyPostCountAsync(request.AuthorId, ct);
        if (dailyCount >= 3)
            throw new PostLimitExceededException();

        var category = Enum.Parse<PostCategory>(request.Category);
        if (category == PostCategory.Lost)
        {
            var hasActive = await _postRepository.HasActiveLostPostAsync(request.AuthorId, ct);
            if (hasActive)
                throw new AlertCooldownException();
        }

        var zone = LocationZone.Create(request.Municipality, request.Settlement);
        GpsCoordinates? coords = request.IncidentLatitude.HasValue
            ? GpsCoordinates.Create(request.IncidentLatitude.Value, request.IncidentLongitude!.Value)
            : null;

        var post = Post.Create(request.AuthorId, category, request.Title,
            request.Content, request.MediaUrls, zone, request.PetId, coords);

        await _postRepository.AddAsync(post, ct);

        foreach (var domainEvent in post.DomainEvents)
            await _publisher.Publish(domainEvent, ct);
        post.ClearDomainEvents();

        return new CreatePostResponse(
            post.Id,
            post.Status.ToString(),
            category == PostCategory.Lost,
            post.ExpiresAt
        );
    }
}

// Application/Commands/Posts/CreatePost/CreatePostValidator.cs
public class CreatePostValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Naslov je obavezan.")
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Opis je obavezan.")
            .MaximumLength(2000);

        RuleFor(x => x.MediaUrls)
            .NotEmpty().WithMessage("Potrebna je najmanje jedna fotografija.");

        RuleFor(x => x.Municipality)
            .NotEmpty().WithMessage("Lokacija je obavezna.");

        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(c => Enum.TryParse<PostCategory>(c, out _))
            .WithMessage("Nevažeća kategorija.");
    }
}
```

### Query struktura

```csharp
// Application/Queries/Posts/GetFeed/GetFeedQuery.cs
public record GetFeedQuery(
    Guid UserId,
    string Municipality,
    PostCategory? CategoryFilter,
    string? Cursor,   // Base64 enkodiran timestamp za cursor pagination
    int PageSize = 20
) : IRequest<GetFeedResponse>;

public record GetFeedResponse(
    IEnumerable<PostSummaryDto> Posts,
    string? NextCursor,
    bool HasMore
);

// Application/DTOs/PostSummaryDto.cs
public record PostSummaryDto(
    Guid Id,
    string Category,
    string Title,
    string Content,
    string? PrimaryImageUrl,
    string LocationZone,
    string AuthorDisplayName,
    string AuthorTrustCategory,  // Prikazujemo kategoriju, ne tačan broj
    bool IsUrgent,
    string Status,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    int SightingCount   // Za Lost objave
);
```

---

## Domain Event Handlers

```csharp
// Application/EventHandlers/AmberAlertActivatedHandler.cs
public class AmberAlertActivatedHandler : INotificationHandler<AmberAlertActivatedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IPostRepository _postRepository;

    public AmberAlertActivatedHandler(IUserRepository userRepository,
        INotificationService notificationService, IPostRepository postRepository)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _postRepository = postRepository;
    }

    public async Task Handle(AmberAlertActivatedEvent notification, CancellationToken ct)
    {
        IEnumerable<Guid> recipientIds;

        if (notification.Location.HasValue)
            recipientIds = await _userRepository.GetUsersInRadiusAsync(
                notification.Location.Value, 10_000, ct);
        else
            recipientIds = await _userRepository.GetUsersByZoneAsync(
                notification.Zone, ct);

        recipientIds = recipientIds.Where(id => id != notification.ReporterId);

        var post = await _postRepository.GetByIdAsync(notification.PostId, ct);
        if (post is null) return;

        await _notificationService.SendPushToManyAsync(
            recipientIds,
            title: "Izgubljen pas u vašem kraju",
            body: $"{post.Title}. Jeste li ga videli?",
            data: new Dictionary<string, string>
            {
                { "postId", notification.PostId.ToString() },
                { "type", "amber_alert" }
            },
            ct);
    }
}

// Application/EventHandlers/PostResolvedHandler.cs
public class PostResolvedHandler : INotificationHandler<PostResolvedEvent>
{
    private readonly IUserRepository _userRepository;

    public PostResolvedHandler(IUserRepository userRepository)
        => _userRepository = userRepository;

    public async Task Handle(PostResolvedEvent notification, CancellationToken ct)
    {
        if (notification.Category == PostCategory.Lost)
        {
            var user = await _userRepository.GetByIdAsync(notification.AuthorId, ct);
            user?.ApplyTrustScoreChange(TrustScoreAction.LostAlertResolved);
            if (user is not null)
                await _userRepository.UpdateAsync(user, ct);
        }
    }
}
```

---

## Pipeline Behaviors

```csharp
// Application/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

---

## API Controllers

```csharp
// API/Controllers/PostsController.cs
[ApiController]
[Route("api/v1/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetFeed([FromQuery] GetFeedRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var query = new GetFeedQuery(userId, request.Municipality,
            request.Category, request.Cursor, request.PageSize);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost(
        [FromBody] CreatePostRequest request, CancellationToken ct)
    {
        var command = new CreatePostCommand(
            GetCurrentUserId(),
            request.Category, request.Title, request.Content,
            request.MediaUrls, request.Municipality, request.Settlement,
            request.IncidentLatitude, request.IncidentLongitude, request.PetId);

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetPost), new { id = result.PostId }, result);
    }

    [HttpPost("{id}/report")]
    [Authorize]
    public async Task<IActionResult> ReportPost(Guid id,
        [FromBody] ReportPostRequest request, CancellationToken ct)
    {
        var command = new ReportPostCommand(id, GetCurrentUserId(), request.Reason, request.Evidence);
        await _mediator.Send(command, ct);
        return NoContent();
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
```

---

## API Error Format (konzistentan kroz ceo projekat)

```csharp
// API/Middleware/ErrorHandlingMiddleware.cs
// Svi errori vraćaju ovaj format:
{
    "error": {
        "code": "POST_LIMIT_EXCEEDED",
        "message": "Maksimalno 3 objave dnevno.",
        "details": {}
    }
}

// Error codes koji se koriste:
// DOMAIN_ERROR            — generički domenski izuzetak
// POST_LIMIT_EXCEEDED     — max 3 objave/dan
// ALERT_COOLDOWN          — max 1 Lost alert/24h
// UNAUTHORIZED_POST_ACTION — nije autor objave
// POST_ALREADY_RESOLVED   — objava je već zatvorena
// VALIDATION_ERROR        — FluentValidation greška
// NOT_FOUND               — entitet ne postoji
// UNAUTHORIZED            — JWT problem
```

---

## SignalR Hubovi

```csharp
// API/Hubs/ChatHub.cs
[Authorize]
public class ChatHub : Hub
{
    public async Task JoinConversation(string conversationId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");

    public async Task LeaveConversation(string conversationId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
}
// Slanje poruke: Clients.Group($"conv_{conversationId}").SendAsync("NewMessage", messageDto)

// API/Hubs/AlertHub.cs
public class AlertHub : Hub
{
    // Korisnici se automatski dodaju u grupu po opštini pri konekciji
    public override async Task OnConnectedAsync()
    {
        var municipality = Context.GetHttpContext()?.Request.Query["municipality"];
        if (!string.IsNullOrEmpty(municipality))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"zone_{municipality}");
        await base.OnConnectedAsync();
    }
}
// Slanje alerta: Clients.Group($"zone_{municipality}").SendAsync("AmberAlert", alertDto)
```

---

## Hangfire Background Jobs

```csharp
// Infrastructure/BackgroundJobs/ExpiredPostsJob.cs
public class ExpiredPostsJob
{
    // Pokreće se svaki sat
    // Nalazi Lost objave gde ExpiresAt < UtcNow i Status = Active
    // Menja status na Expired
    // Šalje in-app notifikaciju autoru: "Oglas ističe, produžite ili zatvorite"
}

// Infrastructure/BackgroundJobs/SlowResponseJob.cs
public class SlowResponseJob
{
    // Pokreće se svaki dan
    // Nalazi korisnike sa aktivnim oglasom koji nisu odgovorili >48h
    // Oduzima TrustScore: SlowResponse (-5)
}
```

---

## EF Core Konfiguracija (primeri)

```csharp
// Infrastructure/Persistence/Configurations/PostConfiguration.cs
public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Content).HasMaxLength(2000).IsRequired();

        // Value Object — LocationZone kao Owned Entity
        builder.OwnsOne(p => p.LocationZone, lo =>
        {
            lo.Property(l => l.Municipality).HasColumnName("Municipality").HasMaxLength(100);
            lo.Property(l => l.Settlement).HasColumnName("Settlement").HasMaxLength(100);
        });

        // GpsCoordinates kao Owned Entity (nullable)
        builder.OwnsOne(p => p.IncidentLocation, go =>
        {
            go.Property(g => g.Latitude).HasColumnName("IncidentLatitude");
            go.Property(g => g.Longitude).HasColumnName("IncidentLongitude");
        });

        // JSON kolona za MediaUrls
        builder.Property(p => p.MediaUrls)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!
            );

        // Indexi
        builder.HasIndex(p => new { p.AuthorId, p.CreatedAt });
        builder.HasIndex(p => new { p.Category, p.Status, p.CreatedAt });
        builder.HasIndex(p => p.IsUrgent);

        builder.HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AuthorId);
    }
}
```

---

## Dependency Injection Setup

```csharp
// Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}

// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INotificationService, FcmNotificationService>();
        services.AddScoped<IImageHashingService, ImageHashingService>();

        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseSqlServerStorage(config.GetConnectionString("DefaultConnection")));

        services.AddHangfireServer();

        return services;
    }
}
```
