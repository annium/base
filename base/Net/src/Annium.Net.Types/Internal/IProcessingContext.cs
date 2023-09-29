using System;
using System.Collections.Generic;
using Annium.Net.Types.Internal.Config;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal interface IMapperProcessingContext : IProcessingContext
{
    IReadOnlyCollection<IModel> GetModels();
}

internal interface IProcessingContext
{
    IMapperConfigInternal Config { get; }
    IReadOnlyCollection<ContextualType> GetImplementations(ContextualType type);
    void Process(ContextualType type, Nullability nullability);
    void Process(ContextualType type);
    IRef GetRef(ContextualType type, Nullability nullability);
    IRef GetRef(ContextualType type);
    IRef RequireRef(ContextualType type);
    bool IsRegistered(Type type);
    void Register(Type type, IModel model);
}