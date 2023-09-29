using System;
using Annium.Core.Mapper;

namespace Annium.Data.Tables;

public static class ChangeEventExtensions
{
    public static IChangeEvent<TOut> MapChangeEvent<TIn, TOut>(this IMapper mapper, IChangeEvent<TIn> e) => e switch
    {
        InitEvent<TIn> x   => ChangeEvent.Init(mapper.Map<TOut[]>(x.Values)),
        AddEvent<TIn> x    => ChangeEvent.Add(mapper.Map<TOut>(x.Value)),
        UpdateEvent<TIn> x => ChangeEvent.Update(mapper.Map<TOut>(x.Value)),
        DeleteEvent<TIn> x => ChangeEvent.Delete(mapper.Map<TOut>(x.Value)),
        _                  => throw new NotImplementedException()
    };
}