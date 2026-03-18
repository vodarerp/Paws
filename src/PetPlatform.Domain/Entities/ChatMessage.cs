using PetPlatform.Domain.Common;
using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = default!;
    public bool IsRead { get; private set; }

    public ChatConversation Conversation { get; private set; } = default!;
    public User Sender { get; private set; } = default!;

    protected ChatMessage() { }

    public static ChatMessage Create(Guid conversationId, Guid senderId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Sadržaj poruke ne može biti prazan.", "EMPTY_MESSAGE");

        return new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content.Trim()
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        SetUpdated();
    }
}
