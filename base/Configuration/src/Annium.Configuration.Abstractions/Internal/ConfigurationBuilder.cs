using System.Collections.Generic;
using Annium.Core.Mapper;
using Annium.Core.Runtime.Types;

namespace Annium.Configuration.Abstractions.Internal;

internal class ConfigurationBuilder : IConfigurationBuilder
{
    private readonly ITypeManager _typeManager;
    private readonly IMapper _mapper;
    private readonly IConfigurationContainer _container;

    public ConfigurationBuilder(
        ITypeManager typeManager,
        IMapper mapper
    )
    {
        _typeManager = typeManager;
        _mapper = mapper;
        _container = new ConfigurationContainer();
    }

    internal ConfigurationBuilder(
        ITypeManager typeManager,
        IMapper mapper,
        IConfigurationContainer container
    )
    {
        _typeManager = typeManager;
        _mapper = mapper;
        _container = container;
    }

    public IConfigurationContainer Add(IReadOnlyDictionary<string[], string> config) => _container.Add(config);

    public IReadOnlyDictionary<string[], string> Get() => _container.Get();

    public T Build<T>()
        where T : new()
    {
        var config = _container.Get();

        var processor = new ConfigurationProcessor<T>(_typeManager, _mapper, config);

        return processor.Process();
    }
}