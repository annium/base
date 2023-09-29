using System;

namespace Annium.Core.Mapper;

public interface IMapper
{
    bool HasMap<T>(object? source);

    bool HasMap(object? source, Type? type);

    T Map<T>(object? source);

    object? Map(object? source, Type type);
}