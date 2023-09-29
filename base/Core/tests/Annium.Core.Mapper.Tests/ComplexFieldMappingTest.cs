using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Data.Models.Extensions;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests;

public class ComplexFieldMappingTest : TestBase
{
    public ComplexFieldMappingTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile(ConfigureProfile));
    }

    [Fact]
    public void AssignmentMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var serialized = JsonSerializer.Serialize(new { Name = "Alex", Age = 20 });
        var value = new A { SerializedValue = serialized };

        // act
        var result = mapper.Map<B>(value);
        var restored = mapper.Map<A>(result);

        // assert
        result.IsShallowEqual(new B
        {
            IgnoredA = 0,
            IgnoredB = 0,
            Name = "Alex",
            Age = 20,
        });
        restored.IsShallowEqual(new A
        {
            SerializedValue = serialized,
        });
    }

    [Fact]
    public void ConstructorMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var serialized = JsonSerializer.Serialize(new { Name = "Alex", Age = 20 });
        var value = new A { SerializedValue = serialized };

        // act
        var result = mapper.Map<C>(value);
        var restored = mapper.Map<A>(result);

        // assert
        result.IsShallowEqual(new C(0, 0, "Alex", 20));
        restored.IsShallowEqual(new A
        {
            SerializedValue = serialized,
        });
    }

    private void ConfigureProfile(Profile p)
    {
        p.Map<A, B>()
            .Ignore(x => new { x.IgnoredA, x.IgnoredB })
            .For(x => new { x.Name, x.Age }, x => JsonSerializer.Deserialize<Serialized>(x.SerializedValue, default(JsonSerializerOptions)));
        p.Map<B, A>()
            .For(x => x.SerializedValue, x => JsonSerializer.Serialize(new { x.Name, x.Age }, default(JsonSerializerOptions)));
        p.Map<A, C>()
            .Ignore(x => new { x.IgnoredA, x.IgnoredB })
            .For(x => new { x.Name, x.Age }, x => JsonSerializer.Deserialize<Serialized>(x.SerializedValue, default(JsonSerializerOptions)));
        p.Map<C, A>()
            .For(x => x.SerializedValue, x => JsonSerializer.Serialize(new { x.Name, x.Age }, default(JsonSerializerOptions)));
    }

    private class A
    {
        public string SerializedValue { get; set; } = string.Empty;
    }

    private class B
    {
        public int IgnoredA { get; set; }
        public long IgnoredB { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class C
    {
        public int IgnoredA { get; }
        public long IgnoredB { get; }
        public string Name { get; }
        public int Age { get; }

        public C(int ignoredA, long ignoredB, string name, int age)
        {
            IgnoredA = ignoredA;
            IgnoredB = ignoredB;
            Name = name;
            Age = age;
        }
    }

    private class Serialized
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}