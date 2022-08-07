using Domain;
using System.Text.Json.Serialization;

namespace Infrastructure.Context;

public sealed class DataObject<T> : IDataObject<T> where T : Entity
{
    public DataObject(
        string id,
        string partitionKey,
        string type,
        T data,
        string etag,
        int ttl,
        EntityState state = EntityState.Unmodified)
    {
        Id = id;
        PartitionKey = partitionKey;
        Type = type;
        Data = data;
        Etag = etag;
        Ttl = ttl;
        State = state;
    }

    public string Id { get; private set; }

    public string PartitionKey { get; private set; }

    public string Type { get; private set; }

    public T Data { get; set; }

    [JsonPropertyName("_etag")] public string Etag { get; set; }

    public int Ttl { get; }
    [JsonIgnore] public EntityState State { get; set; }
}
