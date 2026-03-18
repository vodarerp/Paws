namespace PetPlatform.Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message, string? code = null)
        : base(message)
    {
        Code = code ?? "DOMAIN_ERROR";
    }
}

public class PostLimitExceededException()
    : DomainException("Maksimalno 3 objave dnevno.", "POST_LIMIT_EXCEEDED");

public class AlertCooldownException()
    : DomainException("Samo 1 prijava nestanka u 24 sata.", "ALERT_COOLDOWN");

public class UnauthorizedPostAccessException()
    : DomainException("Nemate pristup ovoj objavi.", "UNAUTHORIZED_POST_ACTION");

public class PostAlreadyResolvedException()
    : DomainException("Ova objava je već zatvorena.", "POST_ALREADY_RESOLVED");

public class InvalidPostCategoryException()
    : DomainException("Nevažeća kategorija objave.", "INVALID_POST_CATEGORY");
