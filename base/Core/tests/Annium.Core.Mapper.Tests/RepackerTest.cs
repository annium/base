using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.Runtime;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests for repacking functionality in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the repacker can repack different expression types
/// </remarks>
public class RepackerTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RepackerTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public RepackerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that binary expressions can be repacked correctly
    /// </summary>
    [Fact]
    public void Binary_Works()
    {
        // act
        var result = Repack<int?, int>(v => v ?? default);

        // assert
        result(1).Is(1);
        result(null).Is(0);
    }

    /// <summary>
    /// Verifies that method call expressions can be repacked correctly
    /// </summary>
    [Fact]
    public void Call_Works()
    {
        // act
        var result = Repack<int, string>(v => v.ToString());

        // assert
        result(1).Is("1");
    }

    /// <summary>
    /// Verifies that lambda expressions can be repacked correctly
    /// </summary>
    [Fact]
    public void Lambda_Works()
    {
        // act
        var result = Repack<int, int>(v => v);

        // assert
        result(1).Is(1);
    }

    /// <summary>
    /// Verifies that member access expressions can be repacked correctly
    /// </summary>
    [Fact]
    public void Member_Works()
    {
        // act
        var result = Repack<string, int>(v => v.Length);

        // assert
        result("asd").Is(3);
    }

    /// <summary>
    /// Verifies that member initialization expressions can be repacked correctly
    /// </summary>
    [Fact]
    public void MemberInit_Works()
    {
        // act
        var result = Repack<int, string>(v => new string(' ', v));

        // assert
        result(3).Is("   ");
    }

    /// <summary>
    /// Verifies that ternary conditional expressions can be repacked correctly
    /// </summary>
    [Fact]
    public void Ternary_Works()
    {
        // act
        var result = Repack<string, string>(v =>
            v == "1" ? "one"
            : v == "2" ? "two"
            : "other"
        );

        // assert
        result("1").Is("one");
        result("2").Is("two");
        result("3").Is("other");
    }

    /// <summary>
    /// Verifies that new object expressions can be repacked correctly
    /// </summary>
    [Fact]
    public void New_Works()
    {
        // act
        var result = Repack<string, Exception>(v => new Exception(v) { Source = v });

        // assert
        var ex = result("a");
        ex.Message.Is("a");
        ex.Source.Is("a");
    }

    /// <summary>
    /// Verifies that unary expressions can be repacked correctly
    /// </summary>
    [Fact]
    public void Unary_Works()
    {
        // act
        var result = Repack<bool, bool>(v => !v);

        // assert
        result(false).IsTrue();
    }

    /// <summary>
    /// Verifies that list initialization expressions can be repacked correctly
    /// </summary>
    [Fact]
    public void ListInit_Works()
    {
        // act
        var result = Repack<int, List<int>>(x => new List<int> { x });

        // assert
        result(5).Has(1).At(0).Is(5);
    }

    /// <summary>
    /// Verifies that array initialization expressions can be repacked correctly
    /// </summary>
    [Fact]
    public void NewArrayInit_Works()
    {
        // act
        var result = Repack<int, int[]>(x => new[] { x });

        // assert
        result(5).Has(1).At(0).Is(5);
    }

    /// <summary>
    /// Repacks an expression and returns a compiled function
    /// </summary>
    /// <typeparam name="TS">The source type</typeparam>
    /// <typeparam name="TR">The result type</typeparam>
    /// <param name="ex">The expression to repack</param>
    /// <returns>A compiled function representing the repacked expression</returns>
    private Func<TS, TR> Repack<TS, TR>(Expression<Func<TS, TR>> ex)
    {
        var repacker = new ServiceContainer()
            .AddRuntime(Assembly.GetCallingAssembly())
            .AddMapper(false)
            .BuildServiceProvider()
            .Resolve<IRepacker>();

        var param = Expression.Parameter(typeof(TS));

        return ((Expression<Func<TS, TR>>)repacker.Repack(ex)(param)).Compile();
    }
}
