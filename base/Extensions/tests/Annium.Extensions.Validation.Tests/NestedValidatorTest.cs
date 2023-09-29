using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Validation.Tests;

public class NestedValidatorTest : TestBase
{
    [Fact]
    public async Task Validation_InheritedValidatorWorks()
    {
        // arrange
        var validator = GetValidator<Foo>();

        // act
        var result = await validator.ValidateAsync(new Foo());

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Has(2);
        result.LabeledErrors.At(nameof(Foo.X)).At(0).Is("Value is less, than given minimum");
        result.LabeledErrors.At(nameof(Foo.Y)).At(0).Is("Value is less, than given minimum");
    }

    public class FooValidator : BarValidator<Foo>
    {
        public FooValidator()
        {
            Field(x => x.Y).GreaterThan(1);
        }
    }

    public class BarValidator<T> : Validator<T>
        where T : Bar
    {
        public BarValidator()
        {
            Field(x => x.X).GreaterThan(1);
        }
    }

    public class Foo : Bar
    {
        public int Y { get; set; }
    }

    public class Bar
    {
        public int X { get; set; }
    }
}