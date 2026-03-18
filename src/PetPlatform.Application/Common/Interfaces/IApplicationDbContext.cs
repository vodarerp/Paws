using Microsoft.EntityFrameworkCore;
using PetPlatform.Domain.Entities;

namespace PetPlatform.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Pet> Pets { get; }
    DbSet<Post> Posts { get; }
    DbSet<PostSighting> PostSightings { get; }
    DbSet<Media> Media { get; }
    DbSet<ChatConversation> ChatConversations { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<TrustScoreEntry> TrustScoreHistory { get; }
    DbSet<Report> Reports { get; }
    DbSet<UserNotificationSettings> UserNotificationSettings { get; }
    DbSet<FcmToken> FcmTokens { get; }
    DbSet<InAppNotification> InAppNotifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
