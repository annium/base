// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

/// <summary>
/// Marker interface for commands that use three configuration types
/// </summary>
/// <typeparam name="T1">The first configuration type</typeparam>
/// <typeparam name="T2">The second configuration type</typeparam>
/// <typeparam name="T3">The third configuration type</typeparam>
public interface IConfigurationTypes<T1, T2, T3>;

/// <summary>
/// Marker interface for commands that use two configuration types
/// </summary>
/// <typeparam name="T1">The first configuration type</typeparam>
/// <typeparam name="T2">The second configuration type</typeparam>
public interface IConfigurationTypes<T1, T2>;

/// <summary>
/// Marker interface for commands that use a single configuration type
/// </summary>
/// <typeparam name="T1">The configuration type</typeparam>
public interface IConfigurationTypes<T1>;
