namespace Annium;

/// <summary>
/// Defines a method for creating a copy of an object.
/// </summary>
/// <typeparam name="T">The type of the object to copy.</typeparam>
public interface ICopyable<out T>
{
    /// <summary>
    /// Creates a copy of the current object.
    /// </summary>
    /// <returns>A copy of the current object.</returns>
    T Copy();
}
