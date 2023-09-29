using System.Collections.Generic;

namespace Annium.Localization.Abstractions;

public interface ILocalizer<T> : ILocalizer
{
}

public interface ILocalizer
{
    string this[string entry] { get; }
    string this[string entry, params object[] arguments] { get; }
    string this[string entry, IEnumerable<object> arguments] { get; }
}