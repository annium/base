using System.Collections.Generic;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Data.Models.Extensions;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class DictionaryAssignmentMapResolverTest : TestBase
{
    public DictionaryAssignmentMapResolverTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile(ConfigureProfile));
    }

    [Fact]
    public void ConstructorMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var serialized = JsonSerializer.Serialize(new { Name = "Alex", Age = 20 });
        var value = new Dictionary<string, object> { { "Serialized", serialized } };

        // act
        var result = mapper.Map<C>(value);

        // assert
        result.IsShallowEqual(new C { Name = "Alex", Age = 20 });
    }

    private void ConfigureProfile(Profile p)
    {
        p.Map<Dictionary<string, object>, C>()
            .Ignore(x => new { x.IgnoredA, x.IgnoredB })
            .For(
                x => new { x.Name, x.Age },
                x => JsonSerializer.Deserialize<Info>(
                    x["Serialized"].ToString()!,
                    default(JsonSerializerOptions)
                )
            );
    }

    private class C
    {
        public int IgnoredA { get; set; }
        public long IgnoredB { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class Info
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}