using System.Net;
using Domain;
using Infrastructure.Context;
using Infrastructure.Exceptions;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    public IContainerContext Context { get; }

    private readonly IContactPartitionKeyProvider _partitionKeyProvider;
    private const string CONTACT_TYPE = "contact";

    public ContactRepository(IContainerContext context, IContactPartitionKeyProvider partitionKeyProvider)
    {
        Context = context;
        _partitionKeyProvider = partitionKeyProvider;
    }

    public void Create(Contact contact)
    {
        var dataObject = new DataObject<Contact>(
            contact.Id.ToString(),
            _partitionKeyProvider.GetPartitionKey(contact),
            CONTACT_TYPE,
            contact,
            null,
            -1,
            EntityState.Created);

        Context.Add(dataObject);
    }

    public async Task DeleteAsync(Guid id, string etag)
    {
        try
        {
            var result = await Context.Container.ReadItemAsync<DataObject<Contact>>(
               id.ToString(),
               new PartitionKey(_partitionKeyProvider.GetPartitionKey(id.ToString())));

            var dataObject = result.Resource;

            dataObject.Data.SetDeleted();
            dataObject.State = EntityState.Deleted;
            dataObject.Etag = string.IsNullOrWhiteSpace(etag) ? result.ETag : etag;

            Context.Add(dataObject);

        }
        catch (CosmosException e)
        {
            throw EvaluateCosmosError(e, id, etag);
        }
    }

    public async Task<(List<(Contact, string)>, bool, string)> ReadAllAsync(int pageSize, string continuationToken)
    {
        var contacts = new List<(Contact, string)>();

        const string sqlQueryText = "SELECT * FROM c WHERE c.type = @type AND c.data.deleted = false";

        var queryDefinition = new QueryDefinition(sqlQueryText).WithParameter("@type", CONTACT_TYPE);

        var queryResultSetIterator = Context.Container.GetItemQueryIterator<DataObject<Contact>>(
            queryDefinition,
            string.IsNullOrWhiteSpace(continuationToken) ? null : continuationToken,
            new QueryRequestOptions { MaxItemCount = pageSize }
        );


        try
        {
            if (!queryResultSetIterator.HasMoreResults)
            {
                return (new List<(Contact, string)>(), false, null);
            }

            var currentResultSet = await queryResultSetIterator.ReadNextAsync();

            contacts.AddRange(currentResultSet.Select(o => (o.Data, o.Etag)));

            return (contacts, currentResultSet.ContinuationToken != null, currentResultSet.ContinuationToken);
        }
        catch (CosmosException e)
        {
            throw EvaluateCosmosError(e);
        }
    }

    public async Task<(Contact, string)> ReadAsync(Guid id, string etag)
    {
        var requestOptions = new ItemRequestOptions();

        if (!string.IsNullOrWhiteSpace(etag))
        {
            requestOptions.IfNoneMatchEtag = etag;
        }

        try
        {
            var result = await Context.Container.ReadItemAsync<DataObject<Contact>>(
                id.ToString(),
                new PartitionKey(_partitionKeyProvider.GetPartitionKey(id.ToString())),
                requestOptions);

            if (result.Resource.Data.Deleted)
            {
                throw new DomainObjectNotFoundException($"Domain object not found for Id: {id} / ETag: {etag}");
            }

            return (result.Resource.Data, result.ETag);
        }
        catch (CosmosException ex)
        {
            throw EvaluateCosmosError(ex, id, etag);
        }
    }

    public void Update(Contact contact, string etag)
    {
        var dataObject = new DataObject<Contact>(
            contact.Id.ToString(),
            _partitionKeyProvider.GetPartitionKey(contact),
            CONTACT_TYPE,
            contact,
            null,
            -1,
            EntityState.Updated)
        {
            Etag = etag,
            Data = contact
        };

        Context.Add(dataObject);
    }

    private Exception EvaluateCosmosError(CosmosException error, Guid? id = null, string etag = null)
    {
        return error.StatusCode switch
        {
            HttpStatusCode.NotFound => new DomainObjectNotFoundException(
                $"Domain object not found for Id: {(id != null ? id.Value : string.Empty)} / ETag: {etag}"),
            HttpStatusCode.NotModified => new DomainObjectNotModifiedException(
                $"Domain object not modified. Id: {(id != null ? id.Value : string.Empty)} / ETag: {etag}"),
            HttpStatusCode.Conflict => new DomainObjectConflictException(
                $"Domain object mid-air collision detected. Id: {(id != null ? id.Value : string.Empty)} / ETag: {etag}"),
            HttpStatusCode.TooManyRequests => new DomainObjectTooManyRequestsException(
                $"Too many requests occurred. Try again later: ({error.RetryAfter?.Milliseconds ?? -1} ms)"),
            _ => new Exception("Cosmos Exception", error)
        };
    }
}
