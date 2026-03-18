using MediatR;
using PetPlatform.Application.Pets.DTOs;

namespace PetPlatform.Application.Pets.Queries.GetPetById;

public record GetPetByIdQuery(Guid PetId) : IRequest<PetDto>;
