using System.Net;
using Domain;
using Domain.Events;
using Infrastructure.Exceptions;
using MediatR;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Context;

public class CosmosContainerContext : IContainerContext
{
    private readonly IMediator _mediator;

    public CosmosContainerContext(IMediator mediator, Container container)
    {
        _mediator = mediator;
        Container = container;
    }

    public Container Container { get; }

    public List<IDataObject<Entity>> DataObjects { get; } = new();

    public void Add(IDataObject<Entity> entity)
    {
        var index = DataObjects.FindIndex(0, dataObject => dataObject.Id == entity.Id && dataObject.PartitionKey == entity.PartitionKey);

        if (index == -1)
        {
            DataObjects.Add(entity);
        }
    }

    public void Reset() => DataObjects.Clear();

    public async Task<List<IDataObject<Entity>>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        RaiseDomainEvents(DataObjects);

        switch (DataObjects.Count)
        {
            case 1:
                {
                    var result = await SaveSingleAsync(DataObjects[0], cancellationToken);
                    return result;
                }
            case > 1:
                {
                    var result = await SaveInTransactionalBatchAsync(cancellationToken);
                    return result;
                }
            default:
                return new List<IDataObject<Entity>>();
        }
    }

    private async Task<List<IDataObject<Entity>>> SaveInTransactionalBatchAsync(CancellationToken cancellationToken)
    {
        if (DataObjects.Count > 0)
        {
            var partitionKey = new PartitionKey(DataObjects[0].PartitionKey);

            var transactionalBatch = Container.CreateTransactionalBatch(partitionKey);

            DataObjects.ForEach(dataObject =>
            {
                TransactionalBatchItemRequestOptions transactionBatchItemRequestOptions = null;

                if (!string.IsNullOrWhiteSpace(dataObject.Etag))
                {
                    transactionBatchItemRequestOptions = new TransactionalBatchItemRequestOptions { IfMatchEtag = dataObject.Etag };
                }

                switch (dataObject.State)
                {
                    case EntityState.Created:
                        transactionalBatch.CreateItem(dataObject);
                        break;
                    case EntityState.Updated or EntityState.Deleted:
                        transactionalBatch.ReplaceItem(dataObject.Id, dataObject, transactionBatchItemRequestOptions);
                        break;
                }
            });

            var transactionalBatchResult = await transactionalBatch.ExecuteAsync(cancellationToken);

            if (!transactionalBatchResult.IsSuccessStatusCode)
            {
                for (var i = 0; i < DataObjects.Count; i++)
                {
                    if (transactionalBatchResult[i].StatusCode != HttpStatusCode.FailedDependency)
                    {
                        // Not recoverable - clear context
                        DataObjects.Clear();
                        throw EvaluateCosmosError(transactionalBatchResult[i].StatusCode);
                    }
                }
            }

            for (var i = 0; i < DataObjects.Count; i++)
            {
                DataObjects[i].Etag = transactionalBatchResult[i].ETag;
            }
        }

        var result = new List<IDataObject<Entity>>(DataObjects); // return copy of list as result

        // work has been successfully done - reset DataObjects list
        DataObjects.Clear();
        return result;
    }

    private async Task<List<IDataObject<Entity>>> SaveSingleAsync(IDataObject<Entity> dataObject, CancellationToken cancellationToken = default)
    {
        var itemRequestOptions = new ItemRequestOptions
        {
            EnableContentResponseOnWrite = false
        };

        if (!string.IsNullOrWhiteSpace(dataObject.Etag)) itemRequestOptions.IfMatchEtag = dataObject.Etag;

        var partitionKey = new PartitionKey(dataObject.PartitionKey);

        try
        {
            ItemResponse<IDataObject<Entity>> response;

            switch (dataObject.State)
            {
                case EntityState.Created:
                    response = await Container.CreateItemAsync(dataObject, partitionKey, itemRequestOptions, cancellationToken);
                    break;
                case EntityState.Updated:
                case EntityState.Deleted:
                    response = await Container.ReplaceItemAsync(dataObject, dataObject.Id, partitionKey, itemRequestOptions, cancellationToken);
                    break;
                default:
                    DataObjects.Clear();
                    return new List<IDataObject<Entity>>();
            }

            dataObject.Etag = response.ETag;
            var result = new List<IDataObject<Entity>>(1) { dataObject };

            // work has been successfully done - reset DataObjects list
            DataObjects.Clear();
            return result;
        }
        catch (CosmosException e)
        {
            // Not recoverable - clear context
            DataObjects.Clear();
            throw EvaluateCosmosError(e, Guid.Parse(dataObject.Id), dataObject.Etag);
        }
    }


    private void RaiseDomainEvents(List<IDataObject<Entity>> dataObjects)
    {
        var eventEmitters = new List<IEventEmitter<IEvent>>();

        foreach (var dataObject in dataObjects)
        {
            if (dataObject.Data is IEventEmitter<IEvent> eventEmitter)
            {
                eventEmitters.Add(eventEmitter);
            }
        }

        if (eventEmitters.Any())
        {
            foreach (var @event in eventEmitters.SelectMany(eventEmitter => eventEmitter.DomainEvents))
            {
                _mediator.Publish(@event);
            }
        }
    }

    private Exception EvaluateCosmosError(CosmosException error, Guid? id = null, string etag = null)
    {
        return EvaluateCosmosError(error.StatusCode, id, etag);
    }

    private Exception EvaluateCosmosError(HttpStatusCode statusCode, Guid? id = null, string etag = null)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => new DomainObjectNotFoundException(
                $"Domain object not found for Id: {(id != null ? id.Value : string.Empty)} / ETag: {etag}"),
            HttpStatusCode.NotModified => new DomainObjectNotModifiedException(
                $"Domain object not modified. Id: {(id != null ? id.Value : string.Empty)} / ETag: {etag}"),
            HttpStatusCode.Conflict => new DomainObjectConflictException(
                $"Domain object conflict detected. Id: {(id != null ? id.Value : string.Empty)} / ETag: {etag}"),
            HttpStatusCode.PreconditionFailed => new DomainObjectPreconditionFailedException(
                $"Domain object mid-air collision detected. Id: {(id != null ? id.Value : string.Empty)} / ETag: {etag}"),
            HttpStatusCode.TooManyRequests => new DomainObjectTooManyRequestsException(
                "Too many requests occurred. Try again later)"),
            _ => new Exception("Cosmos Exception")
        };
    }
}
