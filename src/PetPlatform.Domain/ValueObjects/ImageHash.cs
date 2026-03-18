using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Domain.ValueObjects;

public readonly record struct ImageHash(string Value)
{
    public static ImageHash Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Nevalidan hash slike.", "INVALID_IMAGE_HASH");

        return new ImageHash(hash);
    }
}
