using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Lib.Mapper;

public interface ITestProvider
{
    IReadOnlyCollection<IModel> Models { get; }
    void ConfigureContainer(IServiceContainer container);
    void Setup(IServiceProvider sp);
    IRef Map(ContextualType type);
}