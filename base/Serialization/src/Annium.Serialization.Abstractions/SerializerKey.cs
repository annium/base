namespace Annium.Serialization.Abstractions;

public record struct SerializerKey
{
    public static SerializerKey Create(string key, string mediaType) => new(key, mediaType);
    public static SerializerKey CreateDefault(string mediaType) => new(Constants.DefaultKey, mediaType);
    public string Key { get; }
    public string MediaType { get; }
    public bool IsDefault => Key == Constants.DefaultKey;

    private SerializerKey(string key, string mediaType)
    {
        Key = key;
        MediaType = mediaType;
    }

    public override string ToString() => $"{Key}:{MediaType}";
}