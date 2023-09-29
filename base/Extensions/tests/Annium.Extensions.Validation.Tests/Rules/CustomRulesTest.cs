using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Validation.Tests.Rules;

public class CustomRulesTest : TestBase
{
    [Fact]
    public async Task WhenRule_ImplementsConditional()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Name = "Max" });
        var resultBad = await validator.ValidateAsync(new Person { Name = "Max", Age = 16 });

        // assert
        resultGood.IsOk.IsTrue();
        resultBad.HasErrors.IsTrue();
        resultBad.LabeledErrors.Has(1);
        resultBad.LabeledErrors.At(nameof(Person.Age)).At(0).Is("Value doesn't match condition");
    }

    [Fact]
    public async Task ThenRule_ImplementsShortCircuit()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var result = await validator.ValidateAsync(new Person());

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At(nameof(Person.Name)).At(0).Is("Value is required");
    }

    [Fact]
    public async Task CustomAsyncRule_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var result = await validator.ValidateAsync(new Person { Name = "ho" });

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At(nameof(Person.Name)).At(0).Is($"{nameof(Person.Name)} value is too short");
    }

    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public uint? Age { get; set; }
    }

    // ReSharper disable once UnusedType.Local
    private class PersonValidator : Validator<Person>
    {
        public PersonValidator()
        {
            Field(p => p.Name)
                .Required()
                .Then()
                .Add(async (context, value) =>
                {
                    await Task.CompletedTask;
                    if (value.Length < 3)
                        context.Error("{0} value is too short", context.Field);
                });
            Field(p => p.Age)
                .When(age => age.HasValue)
                .Must(age => age!.Value > 18);
        }
    }
}