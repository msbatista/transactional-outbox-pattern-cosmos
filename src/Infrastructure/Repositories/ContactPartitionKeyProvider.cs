using Domain;

namespace Infrastructure.Repositories;

public sealed class ContactPartitionKeyProvider
{
    public string GetPartitionKey(Contact contact)
    {
        return $"{contact.Id}";
    }

    public string GetPartitionKey(string id)
    {
        return id;
    }
}
