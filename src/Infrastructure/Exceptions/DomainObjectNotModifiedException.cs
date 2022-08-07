namespace Infrastructure.Exceptions;

public sealed class DomainObjectNotModifiedException : Exception
{
    public DomainObjectNotModifiedException()
    {
    }

    public DomainObjectNotModifiedException(string message) : base(message)
    {
    }

    public DomainObjectNotModifiedException(string message, Exception inner) : base(message, inner)
    {
    }
}
