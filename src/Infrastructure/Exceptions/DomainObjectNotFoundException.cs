namespace Infrastructure.Exceptions;

public sealed class DomainObjectNotFoundException : Exception
{
    public DomainObjectNotFoundException()
    {
    }

    public DomainObjectNotFoundException(string message) : base(message)
    {
    }

    public DomainObjectNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}
