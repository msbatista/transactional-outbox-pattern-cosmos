using Domain.ValueObjects;

namespace Domain.Events;

public sealed class ContactCompanyUpdatedEvent : ContactDomainEvent
{
    public ContactCompanyUpdatedEvent(Guid contactId, Company company) : base(Guid.NewGuid(), contactId, nameof(ContactCompanyUpdatedEvent))
    {
        Company = company;
    }

    public Company Company { get; }


}
