using System;
using System.Collections.Generic;

namespace Annium.Data.Operations;

public interface IResultBase<T> : ICloneableResultBase<T>
{
    T Clear();

    T Error(string error);

    T Error(string label, string error);

    T Errors(params string[] errors);

    T Errors(IReadOnlyCollection<string> errors);

    T Errors(params ValueTuple<string, IReadOnlyCollection<string>>[] errors);

    T Errors(IReadOnlyCollection<KeyValuePair<string, IReadOnlyCollection<string>>> errors);

    T Join(params IResultBase[] results);

    T Join(IReadOnlyCollection<IResultBase> results);
}

public interface ICloneableResultBase<T>
{
    T Copy();
}

public interface IDataResultBase<TD> : IResultBase
{
    TD Data { get; }
}

public interface IResultBase
{
    IReadOnlyCollection<string> PlainErrors { get; }
    string PlainError { get; }
    IReadOnlyDictionary<string, IReadOnlyCollection<string>> LabeledErrors { get; }
    bool IsOk { get; }
    bool HasErrors { get; }
}