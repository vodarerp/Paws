namespace PetPlatform.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public Guid CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }
}
