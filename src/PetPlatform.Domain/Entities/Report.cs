using PetPlatform.Domain.Common;
using PetPlatform.Domain.Enums;

namespace PetPlatform.Domain.Entities;

public class Report : BaseEntity
{
    public Guid ReporterId { get; private set; }
    public ReportTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public ReportReason Reason { get; private set; }
    public string? Description { get; private set; }
    public ReportStatus Status { get; private set; } = ReportStatus.Pending;
    public string? AdminNotes { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    public User Reporter { get; private set; } = default!;

    protected Report() { }

    public static Report Create(Guid reporterId, ReportTargetType targetType,
        Guid targetId, ReportReason reason, string? description = null)
    {
        return new Report
        {
            ReporterId = reporterId,
            TargetType = targetType,
            TargetId = targetId,
            Reason = reason,
            Description = description?.Trim()
        };
    }

    public void Resolve(string? adminNotes = null)
    {
        Status = ReportStatus.Resolved;
        AdminNotes = adminNotes;
        ResolvedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void Dismiss(string? adminNotes = null)
    {
        Status = ReportStatus.Dismissed;
        AdminNotes = adminNotes;
        ResolvedAt = DateTime.UtcNow;
        SetUpdated();
    }
}
