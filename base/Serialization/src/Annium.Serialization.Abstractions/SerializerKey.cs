namespace Annium.Serialization.Abstractions;

/// <summary>
/// Represents a key for identifying serializers by configuration key and media type
/// </summary>
public readonly record struct SerializerKey
{
    /// <summary>
    /// Creates a new serializer key with the specified key and media type
    /// </summary>
    /// <param name="key">The configuration key</param>
    /// <param name="mediaType">The media type</param>
    /// <returns>A new serializer key</returns>
    public static SerializerKey Create(string key, string mediaType) => new(key, mediaType);

    /// <summary>
    /// Creates a new default serializer key for the specified media type
    /// </summary>
    /// <param name="mediaType">The media type</param>
    /// <returns>A new default serializer key</returns>
    public static SerializerKey CreateDefault(string mediaType) => new(Constants.DefaultKey, mediaType);

    /// <summary>
    /// Gets the configuration key
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the media type
    /// </summary>
    public string MediaType { get; }

    /// <summary>
    /// Gets a value indicating whether this is a default serializer key
    /// </summary>
    public bool IsDefault => Key == Constants.DefaultKey;

    private SerializerKey(string key, string mediaType)
    {
        Key = key;
        MediaType = mediaType;
    }

    /// <summary>
    /// Returns a string representation of the serializer key
    /// </summary>
    /// <returns>A string in the format "Key:MediaType"</returns>
    public override string ToString() => $"{Key}:{MediaType}";
}
