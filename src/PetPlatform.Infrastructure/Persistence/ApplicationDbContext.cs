using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Entities;

namespace PetPlatform.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostSighting> PostSightings => Set<PostSighting>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<TrustScoreEntry> TrustScoreHistory => Set<TrustScoreEntry>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<UserNotificationSettings> UserNotificationSettings => Set<UserNotificationSettings>();
    public DbSet<FcmToken> FcmTokens => Set<FcmToken>();
    public DbSet<InAppNotification> InAppNotifications => Set<InAppNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
