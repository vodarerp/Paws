using PetPlatform.Domain.Entities;
using PetPlatform.Domain.Enums;

namespace PetPlatform.Domain.Interfaces.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Post> AddAsync(Post post, CancellationToken ct = default);
    Task UpdateAsync(Post post, CancellationToken ct = default);
    Task<int> GetDailyPostCountAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasActiveLostPostAsync(Guid userId, CancellationToken ct = default);
}
