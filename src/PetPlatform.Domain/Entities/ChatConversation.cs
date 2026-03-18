using PetPlatform.Domain.Common;
using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Domain.Entities;

public class ChatConversation : BaseEntity
{
    public Guid? PostId { get; private set; }
    public Guid Participant1Id { get; private set; }
    public Guid Participant2Id { get; private set; }
    public bool PhoneSharedByP1 { get; private set; }
    public bool PhoneSharedByP2 { get; private set; }
    public DateTime? LastMessageAt { get; private set; }

    public Post? Post { get; private set; }
    public User Participant1 { get; private set; } = default!;
    public User Participant2 { get; private set; } = default!;
    public ICollection<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();

    protected ChatConversation() { }

    public static ChatConversation Create(Guid participant1Id, Guid participant2Id, Guid? postId = null)
    {
        return new ChatConversation
        {
            Participant1Id = participant1Id,
            Participant2Id = participant2Id,
            PostId = postId
        };
    }

    public void SharePhone(Guid userId)
    {
        if (userId == Participant1Id)
            PhoneSharedByP1 = true;
        else if (userId == Participant2Id)
            PhoneSharedByP2 = true;
        else
            throw new DomainException("Samo učesnici konverzacije mogu deliti broj.", "UNAUTHORIZED_PHONE_SHARE");

        SetUpdated();
    }

    public void UpdateLastMessageAt()
    {
        LastMessageAt = DateTime.UtcNow;
        SetUpdated();
    }
}
