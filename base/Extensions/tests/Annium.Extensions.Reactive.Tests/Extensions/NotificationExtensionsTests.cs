using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Extensions;

/// <summary>
/// Tests the property-change observables. All three are extension methods in the System namespace, so a
/// consumer finds them by typing a dot on anything that notifies - and nothing exercised them.
/// </summary>
public class NotificationExtensionsTests
{
    /// <summary>
    /// Any property changing is reported.
    /// </summary>
    [Fact]
    public void WhenAnyPropertyChanges_EveryChange_IsReported()
    {
        // arrange
        var target = new Notifying();
        var count = 0;
        using var subscription = target.WhenAnyPropertyChanges().Subscribe(_ => count++);

        // act
        target.Name = "a";
        target.Age = 1;

        // assert
        count.Is(2);
    }

    /// <summary>
    /// Naming a property reports that one and no other.
    /// </summary>
    [Fact]
    public void WhenPropertyChanges_OnlyThatProperty_IsReported()
    {
        // arrange
        var target = new Notifying();
        var count = 0;
        using var subscription = target.WhenPropertyChanges(x => x.Name).Subscribe(_ => count++);

        // act
        target.Age = 1;
        target.Name = "a";
        target.Age = 2;

        // assert
        count.Is(1, "a change to another property is not this one");
    }

    /// <summary>
    /// The value that arrives is the one the property holds by then.
    /// </summary>
    [Fact]
    public void GetPropertyChanges_CarriesTheNewValue()
    {
        // arrange
        var target = new Notifying();
        var seen = new List<string>();
        using var subscription = target.GetPropertyChanges(x => x.Name).Subscribe(seen.Add);

        // act
        target.Name = "first";
        target.Name = "second";

        // assert
        seen.Has(2).At(0).Is("first");
        seen.At(1).Is("second");
    }

    /// <summary>
    /// Unsubscribing detaches from the target, so nothing arrives afterwards.
    /// </summary>
    [Fact]
    public void Subscription_Disposed_StopsReporting()
    {
        // arrange
        var target = new Notifying();
        var count = 0;
        var subscription = target.WhenAnyPropertyChanges().Subscribe(_ => count++);

        // act
        target.Name = "a";
        subscription.Dispose();
        target.Name = "b";

        // assert
        count.Is(1, "a disposed subscription must stop hearing about changes");
    }
}

/// <summary>
/// A target that notifies about its properties.
/// </summary>
public class Notifying : INotifyPropertyChanged
{
    /// <summary>
    /// Raised when a property changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    /// <summary>
    /// Gets or sets the age.
    /// </summary>
    public int Age
    {
        get => _age;
        set => Set(ref _age, value);
    }

    /// <summary>
    /// The name. IDE0032: backing a property that announces its own assignment, not an auto property.
    /// </summary>
#pragma warning disable IDE0032
    private string _name = string.Empty;

    /// <summary>
    /// The age.
    /// </summary>
    private int _age;
#pragma warning restore IDE0032

    /// <summary>
    /// Assigns a field and announces the property it belongs to.
    /// </summary>
    /// <typeparam name="T">The field's type.</typeparam>
    /// <param name="field">The field to assign.</param>
    /// <param name="value">The value to assign.</param>
    /// <param name="property">The property being assigned, filled in by the compiler.</param>
    private void Set<T>(ref T field, T value, [CallerMemberName] string property = "")
    {
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}
