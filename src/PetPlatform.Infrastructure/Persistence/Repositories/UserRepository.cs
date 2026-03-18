using Microsoft.EntityFrameworkCore;
using PetPlatform.Domain.Entities;
using PetPlatform.Domain.Interfaces.Repositories;

namespace PetPlatform.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(
            u => u.Email == email.Trim().ToLowerInvariant(), ct);

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        return user;
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Guid>> GetUsersInRadiusAsync(
        double latitude, double longitude, double radiusMeters, CancellationToken ct = default)
    {
        // Faza 0: koristi SQL Server geography za spatial upit
        return await _context.Users
            .FromSqlRaw(@"
                SELECT * FROM Users
                WHERE IsBanned = 0
                  AND GpsConsentGiven = 1
                  AND LastKnownLatitude IS NOT NULL
                  AND LastKnownLongitude IS NOT NULL
                  AND geography::Point(LastKnownLatitude, LastKnownLongitude, 4326)
                      .STDistance(geography::Point({0}, {1}, 4326)) <= {2}",
                latitude, longitude, radiusMeters)
            .Select(u => u.Id)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Guid>> GetUsersByZoneAsync(
        string locationZone, CancellationToken ct = default)
    {
        return await _context.Users
            .Where(u => !u.IsBanned && u.LocationZone == locationZone)
            .Select(u => u.Id)
            .ToListAsync(ct);
    }
}
