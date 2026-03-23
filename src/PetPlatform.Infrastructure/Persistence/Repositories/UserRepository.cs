using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PetPlatform.Domain.Entities;
using PetPlatform.Domain.Interfaces.Repositories;

namespace PetPlatform.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private static readonly GeometryFactory GeometryFactory = NetTopologySuite.NtsGeometryServices.Instance
        .CreateGeometryFactory(srid: 4326);

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
        var searchPoint = GeometryFactory.CreatePoint(new Coordinate(longitude, latitude));

        return await _context.Users
            .Where(u => !u.IsBanned
                && u.GpsConsentGiven
                && u.Location != null
                && u.Location.Distance(searchPoint) <= radiusMeters)
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
