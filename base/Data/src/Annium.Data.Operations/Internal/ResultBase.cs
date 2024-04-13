using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Annium.Linq;

namespace Annium.Data.Operations.Internal;

internal abstract record ResultBase<T> : IResultBase<T>, IResultBase, ICopyable<T>
    where T : class, IResultBase<T>
{
    public IReadOnlyCollection<string> PlainErrors => _plainErrors;
    public string PlainError => _plainErrors.Join("; ");

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> LabeledErrors =>
        _labeledErrors.ToDictionary(pair => pair.Key, pair => pair.Value as IReadOnlyCollection<string>);

    public bool IsOk => _plainErrors.Count == 0 && _labeledErrors.Count == 0;
    public bool HasErrors => _plainErrors.Count > 0 || _labeledErrors.Count > 0;
    private readonly object _locker = new();
    private readonly HashSet<string> _plainErrors = new();
    private readonly Dictionary<string, HashSet<string>> _labeledErrors = new();

    public abstract T Copy();

    public T Clear()
    {
        lock (_locker)
        {
            _plainErrors.Clear();
            _labeledErrors.Clear();
        }

        return (this as T)!;
    }

    public T Error(string error)
    {
        lock (_locker)
            _plainErrors.Add(error);

        return (this as T)!;
    }

    public T Error(string label, string error)
    {
        lock (_locker)
        {
            if (!_labeledErrors.ContainsKey(label))
                _labeledErrors[label] = new HashSet<string>();
            _labeledErrors[label].Add(error);
        }

        return (this as T)!;
    }

    public T Errors(params string[] errors)
    {
        lock (_locker)
            foreach (var error in errors)
                _plainErrors.Add(error);

        return (this as T)!;
    }

    public T Errors(IReadOnlyCollection<string> errors)
    {
        lock (_locker)
            foreach (var error in errors)
                _plainErrors.Add(error);

        return (this as T)!;
    }

    public T Errors(params ValueTuple<string, IReadOnlyCollection<string>>[] errors)
    {
        lock (_locker)
        {
            foreach (var (label, labelErrors) in errors)
            {
                if (!_labeledErrors.ContainsKey(label))
                    _labeledErrors[label] = new HashSet<string>();
                foreach (var error in labelErrors)
                    _labeledErrors[label].Add(error);
            }
        }

        return (this as T)!;
    }

    public T Errors(IReadOnlyCollection<KeyValuePair<string, IReadOnlyCollection<string>>> errors)
    {
        lock (_locker)
        {
            foreach (var (label, labelErrors) in errors)
            {
                if (!_labeledErrors.ContainsKey(label))
                    _labeledErrors[label] = new HashSet<string>();
                foreach (var error in labelErrors)
                    _labeledErrors[label].Add(error);
            }
        }

        return (this as T)!;
    }

    public T Join(params IResultBase[] results)
    {
        foreach (var result in results)
        {
            Errors(result.PlainErrors);
            Errors(result.LabeledErrors);
        }

        return (this as T)!;
    }

    public T Join(IReadOnlyCollection<IResultBase> results)
    {
        foreach (var result in results)
        {
            Errors(result.PlainErrors);
            Errors(result.LabeledErrors);
        }

        return (this as T)!;
    }

    public string ErrorState()
    {
        lock (_locker)
        {
            var sb = new StringBuilder();

            if (_plainErrors.Count > 0)
            {
                sb.Append($"{_plainErrors.Count} plain errors:");
                foreach (var error in _plainErrors)
                    sb.AppendLine($"- {error}");
            }
            else
                sb.Append("no plain errors");

            if (_labeledErrors.Count > 0)
            {
                sb.AppendLine($"{_labeledErrors.Count} labeled errors:");
                foreach (var (label, errors) in _labeledErrors)
                {
                    sb.AppendLine($"- {label}:");
                    foreach (var error in errors)
                        sb.AppendLine($"-- {error}");
                }
            }
            else
                sb.AppendLine("no labeled errors");

            return sb.ToString();
        }
    }

    protected void CloneTo(T clone)
    {
        clone.Errors(PlainErrors);
        clone.Errors(LabeledErrors);
    }
}
