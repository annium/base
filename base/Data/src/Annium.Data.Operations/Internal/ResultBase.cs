using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Annium.Linq;

namespace Annium.Data.Operations.Internal;

/// <summary>
/// Abstract base class for all result types, providing common error handling functionality.
/// This class implements thread-safe error collection and manipulation operations.
/// </summary>
/// <typeparam name="T">The concrete result type that inherits from this base class.</typeparam>
internal abstract record ResultBase<T> : IResultBase<T>, IResultBase, ICopyable<T>
    where T : class, IResultBase<T>
{
    /// <summary>
    /// Gets a read-only collection of plain error messages that are not associated with any specific label.
    /// </summary>
    public IReadOnlyCollection<string> PlainErrors => _plainErrors;

    /// <summary>
    /// Gets a concatenated string of all plain errors separated by "; ".
    /// </summary>
    public string PlainError => _plainErrors.Join("; ");

    /// <summary>
    /// Gets a read-only dictionary of labeled errors, where each label maps to a collection of error messages.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> LabeledErrors =>
        _labeledErrors.ToDictionary(pair => pair.Key, pair => pair.Value as IReadOnlyCollection<string>);

    /// <summary>
    /// Gets a value indicating whether this result has no errors (both plain and labeled).
    /// </summary>
    public bool IsOk => _plainErrors.Count == 0 && _labeledErrors.Count == 0;

    /// <summary>
    /// Gets a value indicating whether this result has any errors (either plain or labeled).
    /// </summary>
    public bool HasErrors => _plainErrors.Count > 0 || _labeledErrors.Count > 0;

    /// <summary>
    /// Thread synchronization lock for ensuring thread-safe access to error collections.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// Internal collection of plain error messages.
    /// </summary>
    private readonly HashSet<string> _plainErrors = new();

    /// <summary>
    /// Internal collection of labeled errors, where each label maps to a set of error messages.
    /// </summary>
    private readonly Dictionary<string, HashSet<string>> _labeledErrors = new();

    /// <summary>
    /// Creates a deep copy of this result instance.
    /// </summary>
    /// <returns>A new instance of type T that is a copy of this result.</returns>
    public abstract T Copy();

    /// <summary>
    /// Clears all errors (both plain and labeled) from this result.
    /// </summary>
    /// <returns>This result instance for method chaining.</returns>
    public T Clear()
    {
        lock (_locker)
        {
            _plainErrors.Clear();
            _labeledErrors.Clear();
        }

        return (this as T)!;
    }

    /// <summary>
    /// Adds a plain error message to this result.
    /// </summary>
    /// <param name="error">The error message to add.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Error(string error)
    {
        lock (_locker)
            _plainErrors.Add(error);

        return (this as T)!;
    }

    /// <summary>
    /// Adds a labeled error message to this result.
    /// </summary>
    /// <param name="label">The label to associate with the error.</param>
    /// <param name="error">The error message to add.</param>
    /// <returns>This result instance for method chaining.</returns>
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

    /// <summary>
    /// Adds multiple plain error messages to this result.
    /// </summary>
    /// <param name="errors">The error messages to add.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Errors(params string[] errors)
    {
        lock (_locker)
            foreach (var error in errors)
                _plainErrors.Add(error);

        return (this as T)!;
    }

    /// <summary>
    /// Adds multiple plain error messages from a collection to this result.
    /// </summary>
    /// <param name="errors">The collection of error messages to add.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Errors(IReadOnlyCollection<string> errors)
    {
        lock (_locker)
            foreach (var error in errors)
                _plainErrors.Add(error);

        return (this as T)!;
    }

    /// <summary>
    /// Adds multiple labeled error groups to this result.
    /// </summary>
    /// <param name="errors">An array of tuples where each tuple contains a label and its associated error messages.</param>
    /// <returns>This result instance for method chaining.</returns>
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

    /// <summary>
    /// Adds multiple labeled error groups from a collection of key-value pairs to this result.
    /// </summary>
    /// <param name="errors">A collection of key-value pairs where each key is a label and each value is a collection of error messages.</param>
    /// <returns>This result instance for method chaining.</returns>
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

    /// <summary>
    /// Joins the errors from multiple result instances into this result.
    /// </summary>
    /// <param name="results">The result instances whose errors should be added to this result.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Join(params IResultBase[] results)
    {
        foreach (var result in results)
        {
            Errors(result.PlainErrors);
            Errors(result.LabeledErrors);
        }

        return (this as T)!;
    }

    /// <summary>
    /// Joins the errors from a collection of result instances into this result.
    /// </summary>
    /// <param name="results">The collection of result instances whose errors should be added to this result.</param>
    /// <returns>This result instance for method chaining.</returns>
    public T Join(IReadOnlyCollection<IResultBase> results)
    {
        foreach (var result in results)
        {
            Errors(result.PlainErrors);
            Errors(result.LabeledErrors);
        }

        return (this as T)!;
    }

    /// <summary>
    /// Gets a detailed string representation of all errors in this result for debugging purposes.
    /// </summary>
    /// <returns>A formatted string containing all plain and labeled errors.</returns>
    public string ErrorState()
    {
        lock (_locker)
        {
            var sb = new StringBuilder();

            if (_plainErrors.Count > 0)
            {
                sb.AppendLine($"{_plainErrors.Count} plain errors:");
                foreach (var error in _plainErrors)
                    sb.AppendLine($"- {error}");
            }
            else
                sb.AppendLine("no plain errors");

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

    /// <summary>
    /// Copies all errors from this result to the specified clone result instance.
    /// </summary>
    /// <param name="clone">The target result instance to copy errors to.</param>
    protected void CloneTo(T clone)
    {
        clone.Errors(PlainErrors);
        clone.Errors(LabeledErrors);
    }
}
