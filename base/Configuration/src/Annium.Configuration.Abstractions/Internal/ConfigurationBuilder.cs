using System.Collections.Generic;
using Annium.Core.Mapper;
using Annium.Core.Runtime.Types;

namespace Annium.Configuration.Abstractions.Internal;

/// <summary>
/// Internal implementation of IConfigurationBuilder that builds configuration objects
/// </summary>
internal class ConfigurationBuilder : IConfigurationBuilder
{
    /// <summary>
    /// Type manager for resolving types during configuration building
    /// </summary>
    private readonly ITypeManager _typeManager;

    /// <summary>
    /// Mapper for converting between different data types
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Configuration container holding the configuration data
    /// </summary>
    private readonly IConfigurationContainer _container;

    /// <summary>
    /// Initializes a new instance of ConfigurationBuilder with a new container
    /// </summary>
    /// <param name="typeManager">Type manager for type resolution</param>
    /// <param name="mapper">Mapper for data conversion</param>
    public ConfigurationBuilder(ITypeManager typeManager, IMapper mapper)
    {
        _typeManager = typeManager;
        _mapper = mapper;
        _container = new ConfigurationContainer();
    }

    /// <summary>
    /// Adds configuration data to the builder's data store.
    /// </summary>
    /// <param name="config">Configuration data to add</param>
    public void Add(IReadOnlyDictionary<string[], string> config) => _container.Add(config);

    /// <summary>
    /// Builds an instance of type T from the configuration data
    /// </summary>
    /// <typeparam name="T">The configuration type to build.</typeparam>
    /// <returns>Configured instance of type T</returns>
    public T Build<T>()
        where T : new()
    {
        var config = _container.Get();

        var processor = new ConfigurationProcessor<T>(_typeManager, _mapper, config);

        return processor.Process();
    }
}
