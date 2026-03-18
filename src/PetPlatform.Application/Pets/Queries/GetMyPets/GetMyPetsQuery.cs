using MediatR;
using PetPlatform.Application.Pets.DTOs;

namespace PetPlatform.Application.Pets.Queries.GetMyPets;

public record GetMyPetsQuery(Guid OwnerId) : IRequest<List<PetDto>>;
