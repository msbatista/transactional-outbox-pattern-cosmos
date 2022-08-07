namespace Infrastructure.Exceptions;

public sealed class DomainObjectPreconditionFailedException : Exception
{
    public DomainObjectPreconditionFailedException()
    {
    }

    public DomainObjectPreconditionFailedException(string message) : base(message)
    {
    }

    public DomainObjectPreconditionFailedException(string message, Exception inner) : base(message, inner)
    {
    }
}
