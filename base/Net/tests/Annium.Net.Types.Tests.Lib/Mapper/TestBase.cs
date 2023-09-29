using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;
using Xunit.Abstractions;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public abstract class TestBase : Testing.Lib.TestBase
{
    protected IReadOnlyCollection<IModel> Models => _testProvider.Models;
    protected readonly IMapperConfig Config;
    private readonly ITestProvider _testProvider;

    protected TestBase(
        ITestProvider testProvider,
        ITestOutputHelper outputHelper
    ) : base(outputHelper)
    {
        _testProvider = testProvider;
        Register(container =>
        {
            container.AddModelMapper();
            testProvider.ConfigureContainer(container);
        });
        Setup(testProvider.Setup);
        Config = Get<IMapperConfig>();
    }

    protected IRef Map(ContextualType type) => _testProvider.Map(type);
}