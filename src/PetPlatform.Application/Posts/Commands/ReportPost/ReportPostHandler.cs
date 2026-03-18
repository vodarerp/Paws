using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Entities;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Application.Posts.Commands.ReportPost;

public class ReportPostHandler : IRequestHandler<ReportPostCommand>
{
    private readonly IApplicationDbContext _context;

    public ReportPostHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ReportPostCommand request, CancellationToken ct)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId, ct)
            ?? throw new KeyNotFoundException("Objava nije pronadjena.");

        var alreadyReported = await _context.Reports
            .AnyAsync(r => r.ReporterId == request.ReporterId
                && r.TargetType == ReportTargetType.Post
                && r.TargetId == request.PostId, ct);

        if (alreadyReported)
            throw new DomainException("Vec ste prijavili ovu objavu.", "ALREADY_REPORTED");

        var reason = Enum.Parse<ReportReason>(request.Reason, true);
        var report = Report.Create(request.ReporterId, ReportTargetType.Post, request.PostId, reason, request.Description);

        _context.Reports.Add(report);
        post.AddReport();

        await _context.SaveChangesAsync(ct);
    }
}
