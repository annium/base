using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Net.Types.Tests.Lib.Mapper;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Mapper;

internal class TestProvider : ITestProvider
{
    public IReadOnlyCollection<IModel> Models => _mapper.GetModels();
    private IModelMapper _mapper = default!;

    public void ConfigureContainer(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
    }

    public void Setup(IServiceProvider sp)
    {
        _mapper = sp.Resolve<IModelMapper>();
    }

    public IRef Map(ContextualType type) => _mapper.Map(type);
}