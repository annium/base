using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Net.Types.Tests.Lib.Mapper;
using Annium.Serialization.Abstractions;
using Namotion.Reflection;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

internal class TestProvider : ITestProvider
{
    public IReadOnlyCollection<IModel> Models
    {
        get
        {
            var raw = _serializer.Serialize(_mapper.GetModels());
            var models = _serializer.Deserialize<IReadOnlyCollection<IModel>>(raw);
            return models;
        }
    }

    private IModelMapper _mapper = default!;
    private ISerializer<string> _serializer = default!;

    public void ConfigureContainer(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddSerializers()
            .WithJson(opts => { opts.ConfigureForNetTypes(); }, isDefault: true);
    }

    public void Setup(IServiceProvider sp)
    {
        _mapper = sp.Resolve<IModelMapper>();
        _serializer = sp.Resolve<ISerializer<string>>();
    }

    public IRef Map(ContextualType type)
    {
        var originalRef = _mapper.Map(type);
        var raw = _serializer.Serialize(originalRef);
        var modelRef = _serializer.Deserialize<IRef>(raw);

        return modelRef;
    }
}