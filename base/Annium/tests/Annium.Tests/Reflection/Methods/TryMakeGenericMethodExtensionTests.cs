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

    /// <summary>
    /// Static helper class providing generic methods used as reflection targets by the
    /// <c>TryMakeGenericMethod</c> tests.
    /// </summary>
    private static class Holder
    {
        /// <summary>
        /// Unconstrained generic identity method; used to test the success path of
        /// <c>TryMakeGenericMethod</c> with a valid type argument.
        /// </summary>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <param name="value">The value to return unchanged.</param>
        /// <returns>The same <paramref name="value"/> passed in.</returns>
        public static T Identity<T>(T value) => value;

        /// <summary>
        /// Generic method constrained to reference types; used to test the constraint-violation path
        /// of <c>TryMakeGenericMethod</c> (passing a value type triggers the catch-returns-false branch).
        /// </summary>
        /// <typeparam name="T">The type parameter constrained to <see langword="class"/>.</typeparam>
        /// <returns>Always <see langword="null"/> — the method body is irrelevant to the tests.</returns>
        public static T ClassOnly<T>()
            where T : class => null!;
    }
}
