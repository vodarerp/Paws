using PetPlatform.Domain.Common;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Domain.Entities;

public class Pet : AuditableEntity
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Breed { get; private set; } = default!;
    public string? Age { get; private set; }
    public PetGender Gender { get; private set; }
    public PetSize Size { get; private set; }
    public string? Color { get; private set; }
    public string? SpecialMarks { get; private set; }
    public string? ChipNumber { get; private set; }
    public bool? IsSterilized { get; private set; }
    public PetStatus Status { get; private set; } = PetStatus.WithOwner;

    public User Owner { get; private set; } = default!;
    public ICollection<Media> Photos { get; private set; } = new List<Media>();

    protected Pet() { }

    public static Pet Create(Guid ownerId, string name, string breed,
        PetGender gender, PetSize size, string? age = null, string? color = null)
    {
        return new Pet
        {
            OwnerId = ownerId,
            Name = name.Trim(),
            Breed = breed.Trim(),
            Gender = gender,
            Size = size,
            Age = age?.Trim(),
            Color = color?.Trim()
        };
    }

    public void MarkAsLost() { Status = PetStatus.Lost; SetUpdated(); }
    public void MarkAsFound() { Status = PetStatus.WithOwner; SetUpdated(); }
    public void MarkForAdoption() { Status = PetStatus.ForAdoption; SetUpdated(); }
}
