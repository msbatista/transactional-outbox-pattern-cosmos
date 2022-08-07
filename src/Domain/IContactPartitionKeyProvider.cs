namespace Domain;

public interface IContactPartitionKeyProvider
{
    string GetPartitionKey(Contact contact);
    string GetPartitionKey(string id);
}
