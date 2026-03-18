using PetPlatform.Domain.Entities;

namespace PetPlatform.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User> AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task<IEnumerable<Guid>> GetUsersInRadiusAsync(double latitude, double longitude, double radiusMeters, CancellationToken ct = default);
    Task<IEnumerable<Guid>> GetUsersByZoneAsync(string locationZone, CancellationToken ct = default);
}
