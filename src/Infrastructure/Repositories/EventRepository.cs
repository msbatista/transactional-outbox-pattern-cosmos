using Domain.Events;
using Infrastructure.Context;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly IConfiguration _configuration;
    private IContainerContext Context { get; }

    private const string EVENT_TYPE = "domainEvent";
    private readonly int DEFAULT_TTL;

    public EventRepository(IConfiguration configuration, IContainerContext context)
    {
        _configuration = configuration;
        Context = context;
        DEFAULT_TTL = _configuration.GetSection("Events")?["Ttl"] == null ? 120 : int.Parse(_configuration.GetSection("Events")?["Ttl"]!);
    }

    public void Create(ContactDomainEvent e)
    {
        var dataObject = new DataObject<ContactDomainEvent>(
            e.Id.ToString(),
            e.ContactId.ToString(),
            EVENT_TYPE,
            e,
            null,
            DEFAULT_TTL,
            EntityState.Created);

        Context.Add(dataObject);
    }
}
