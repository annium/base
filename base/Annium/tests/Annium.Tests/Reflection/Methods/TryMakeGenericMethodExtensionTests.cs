using System;
using System.Reflection;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Methods;

/// <summary>
/// Tests for <see cref="TryMakeGenericMethodExtension.TryMakeGenericMethod"/> covering the happy path,
/// constraint violation (catch-returns-false branch), and arity mismatch.
/// </summary>
public class TryMakeGenericMethodExtensionTests
{
    /// <summary>
    /// Null receiver throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void TryMakeGenericMethod_NullMethod_Throws()
    {
        Wrap.It(() => (null as MethodInfo)!.TryMakeGenericMethod(out _, typeof(int))).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Valid type args produce a closed generic method.
    /// </summary>
    [Fact]
    public void TryMakeGenericMethod_ValidGenericArgs_ReturnsTrue()
    {
        var method = typeof(Holder).GetMethod(nameof(Holder.Identity))!;

        var ok = method.TryMakeGenericMethod(out var result, typeof(int));

        ok.IsTrue();
        result!.IsGenericMethod.IsTrue();
        result.GetGenericArguments()[0].Is(typeof(int));
    }

    /// <summary>
    /// Constraint violation (passing a value type to a `where T : class` method) exercises the
    /// catch-returns-false branch.
    /// </summary>
    [Fact]
    public void TryMakeGenericMethod_ConstraintViolation_ReturnsFalseAndOutNull()
    {
        var method = typeof(Holder).GetMethod(nameof(Holder.ClassOnly))!;

        var ok = method.TryMakeGenericMethod(out var result, typeof(int));

        ok.IsFalse();
        (result is null).IsTrue();
    }

    /// <summary>
    /// Mismatched type-argument arity is caught by the try/catch and surfaced as false.
    /// </summary>
    [Fact]
    public void TryMakeGenericMethod_ArityMismatch_ReturnsFalse()
    {
        var method = typeof(Holder).GetMethod(nameof(Holder.Identity))!;

        var ok = method.TryMakeGenericMethod(out var result, typeof(int), typeof(string));

        ok.IsFalse();
        (result is null).IsTrue();
    }

    private static class Holder
    {
        public static T Identity<T>(T value) => value;

        public static T ClassOnly<T>()
            where T : class => null!;
    }
}
