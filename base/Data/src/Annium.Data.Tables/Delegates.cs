namespace Annium.Data.Tables;

/// <summary>
/// Delegate for extracting a key from a value
/// </summary>
/// <typeparam name="T">The type of the source object</typeparam>
/// <param name="source">The source object</param>
/// <returns>The key value</returns>
public delegate int GetKey<in T>(T source);

/// <summary>
/// Delegate for determining if a source has changed compared to an update value
/// </summary>
/// <typeparam name="TSource">The type of the source object</typeparam>
/// <typeparam name="TUpdate">The type of the update value</typeparam>
/// <param name="source">The source object</param>
/// <param name="value">The update value</param>
/// <returns>True if the source has changed</returns>
public delegate bool HasChanged<in TSource, in TUpdate>(TSource source, TUpdate value);

/// <summary>
/// Delegate for updating a source object with a new value
/// </summary>
/// <typeparam name="TSsource">The type of the source object</typeparam>
/// <typeparam name="TUpdate">The type of the update value</typeparam>
/// <param name="source">The source object to update</param>
/// <param name="value">The new value</param>
public delegate void Update<in TSsource, in TUpdate>(TSsource source, TUpdate value);
