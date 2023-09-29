using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Validation.Tests.Rules;

public class BaseRulesTest : TestBase
{
    [Fact]
    public async Task Required_String_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Name = "some" });
        var resultBad = await validator.ValidateAsync(new Person { Name = " " });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Name)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Name)).At(0).Is("Value is required");
    }

    [Fact]
    public async Task Required_Nullable_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Nullable = null });
        var resultGood2 = await validator.ValidateAsync(new Person { Nullable = 2 });
        var resultBad = await validator.ValidateAsync(new Person { Nullable = 0 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Nullable)).IsFalse();
        resultGood2.LabeledErrors.ContainsKey(nameof(Person.Nullable)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Nullable)).At(0).Is("Value is required");
    }

    [Fact]
    public async Task Required_NotString_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Age = 1 });
        var resultBad = await validator.ValidateAsync(new Person());

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Age)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Age)).At(0).Is("Value is required");
    }

    [Fact]
    public async Task EqualValue_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Fixed = "fixed value" });
        var resultBad = await validator.ValidateAsync(new Person { Fixed = "other value" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Fixed)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Fixed)).At(0).Is("Value is not equal to given");
    }

    [Fact]
    public async Task In_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { OneOf = "one" });
        var resultBad = await validator.ValidateAsync(new Person { OneOf = "three" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.OneOf)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.OneOf)).At(0).Is("Value is not in given");
    }

    [Fact]
    public async Task EqualAccessor_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Name = "some", SameAsName = "some" });
        var resultBad = await validator.ValidateAsync(new Person { Name = "some", SameAsName = "other" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.SameAsName)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.SameAsName)).At(0).Is("Value is not equal to given");
    }

    [Fact]
    public async Task NotEqualValue_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { NotFixed = "other value" });
        var resultBad = await validator.ValidateAsync(new Person { NotFixed = "fixed value" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.NotFixed)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.NotFixed)).At(0).Is("Value is equal to given");
    }

    [Fact]
    public async Task NotIn_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { NotOneOf = "three" });
        var resultBad = await validator.ValidateAsync(new Person { NotOneOf = "one" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.NotOneOf)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.NotOneOf)).At(0).Is("Value is in given");
    }

    [Fact]
    public async Task NotEqualAccessor_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Name = "some", NotSameAsName = "other" });
        var resultBad = await validator.ValidateAsync(new Person { Name = "some", NotSameAsName = "some" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.NotSameAsName)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.NotSameAsName)).At(0).Is("Value is equal to given");
    }

    [Fact]
    public async Task MinMaxLength_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MinMaxLength = "other" });
        var resultBadMin = await validator.ValidateAsync(new Person { MinMaxLength = "x" });
        var resultBadMax = await validator.ValidateAsync(new Person { MinMaxLength = "otherx" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MinMaxLength)).IsFalse();
        resultBadMin.LabeledErrors.At(nameof(Person.MinMaxLength)).At(0).Is("Value length is less, than 2");
        resultBadMax.LabeledErrors.At(nameof(Person.MinMaxLength)).At(0).Is("Value length is greater, than 5");
    }

    [Fact]
    public async Task MinLength_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MinLength = "other" });
        var resultBad = await validator.ValidateAsync(new Person { MinLength = "x" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MinLength)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MinLength)).At(0).Is("Value length is less, than 2");
    }

    [Fact]
    public async Task MaxLength_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MaxLength = "other" });
        var resultBad = await validator.ValidateAsync(new Person { MaxLength = "otherx" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MaxLength)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MaxLength)).At(0).Is("Value length is greater, than 5");
    }

    [Fact]
    public async Task Between_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Between = 2 });
        var resultBadMin = await validator.ValidateAsync(new Person { Between = 1 });
        var resultBadMax = await validator.ValidateAsync(new Person { Between = 4 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Between)).IsFalse();
        resultBadMin.LabeledErrors.At(nameof(Person.Between)).At(0).Is("Value is less, than given minimum");
        resultBadMax.LabeledErrors.At(nameof(Person.Between)).At(0).Is("Value is greater, than given maximum");
    }

    [Fact]
    public async Task LessThan_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { LessThan = 2 });
        var resultBad = await validator.ValidateAsync(new Person { LessThan = 3 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.LessThan)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.LessThan)).At(0).Is("Value is greater, than given maximum");
    }

    [Fact]
    public async Task LessThanOrEqual_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { LessThanOrEqual = 3 });
        var resultBad = await validator.ValidateAsync(new Person { LessThanOrEqual = 4 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.LessThanOrEqual)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.LessThanOrEqual)).At(0).Is("Value is greater, than given maximum");
    }

    [Fact]
    public async Task GreaterThan_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { GreaterThan = 4 });
        var resultBad = await validator.ValidateAsync(new Person { GreaterThan = 3 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.GreaterThan)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.GreaterThan)).At(0).Is("Value is less, than given minimum");
    }

    [Fact]
    public async Task GreaterThanOrEqual_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { GreaterThanOrEqual = 3 });
        var resultBad = await validator.ValidateAsync(new Person { GreaterThanOrEqual = 2 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.GreaterThanOrEqual)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.GreaterThanOrEqual)).At(0).Is("Value is less, than given minimum");
    }

    [Fact]
    public async Task RegexString_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { RegexString = "yes" });
        var resultBad = await validator.ValidateAsync(new Person { RegexString = "yoda" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.RegexString)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.RegexString)).At(0).Is("Value doesn't match specified regex");
    }

    [Fact]
    public async Task RegexInstance_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { RegexInstance = "yes" });
        var resultBad = await validator.ValidateAsync(new Person { RegexInstance = "yoda" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.RegexInstance)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.RegexInstance)).At(0).Is("Value doesn't match specified regex");
    }

    [Fact]
    public async Task MustField_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MustField = "x" });
        var resultBad = await validator.ValidateAsync(new Person { MustField = "y" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MustField)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MustField)).At(0).Is("Value doesn't match condition");
    }

    [Fact]
    public async Task MustValueField_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MustField = "a", MustValueField = "a" });
        var resultBad = await validator.ValidateAsync(new Person { MustField = "a", MustValueField = "b" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MustValueField)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MustValueField)).At(0).Is("Value doesn't match condition");
    }

    [Fact]
    public async Task MustFieldAsync_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MustFieldAsync = "x" });
        var resultBad = await validator.ValidateAsync(new Person { MustFieldAsync = "y" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MustFieldAsync)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MustFieldAsync)).At(0).Is("Value doesn't match condition");
    }

    [Fact]
    public async Task MustValueFieldAsync_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { MustFieldAsync = "a", MustValueFieldAsync = "a" });
        var resultBad = await validator.ValidateAsync(new Person { MustFieldAsync = "a", MustValueFieldAsync = "b" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.MustValueFieldAsync)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.MustValueFieldAsync)).At(0).Is("Value doesn't match condition");
    }

    [Fact]
    public async Task Email_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Email = "yes@xx" });
        var resultBad = await validator.ValidateAsync(new Person { Email = "yoda@" });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Email)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Email)).At(0).Is("Value is not an email");
    }

    [Fact]
    public async Task Enum_Works()
    {
        // arrange
        var validator = GetValidator<Person>();

        // act
        var resultGood = await validator.ValidateAsync(new Person { Enum = LogLevel.Debug | LogLevel.Info });
        var resultBad = await validator.ValidateAsync(new Person { Enum = (LogLevel)8 });

        // assert
        resultGood.LabeledErrors.ContainsKey(nameof(Person.Enum)).IsFalse();
        resultBad.LabeledErrors.At(nameof(Person.Enum)).At(0).Is("Value is not in expected range");
    }

    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public uint Age { get; set; }
        public long? Nullable { get; set; }
        public string Fixed { get; set; } = string.Empty;
        public string OneOf { get; set; } = string.Empty;
        public string SameAsName { get; set; } = string.Empty;
        public string NotFixed { get; set; } = string.Empty;
        public string NotOneOf { get; set; } = string.Empty;
        public string NotSameAsName { get; set; } = string.Empty;
        public string MinMaxLength { get; set; } = string.Empty;
        public string MinLength { get; set; } = string.Empty;
        public string MaxLength { get; set; } = string.Empty;
        public long Between { get; set; }
        public long LessThan { get; set; }
        public long LessThanOrEqual { get; set; }
        public long GreaterThan { get; set; }
        public long GreaterThanOrEqual { get; set; }
        public string RegexString { get; set; } = string.Empty;
        public string RegexInstance { get; set; } = string.Empty;
        public string MustField { get; set; } = string.Empty;
        public string MustValueField { get; set; } = string.Empty;
        public string MustFieldAsync { get; set; } = string.Empty;
        public string MustValueFieldAsync { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public LogLevel Enum { get; set; }
    }

    [Flags]
    private enum LogLevel
    {
        None = 0,
        Trace = 1,
        Debug = 2,
        Info = 4,
    }

    // ReSharper disable once UnusedType.Local
    private class PersonValidator : Validator<Person>
    {
        public PersonValidator()
        {
            Field(p => p.Name).Required();
            Field(p => p.Age).Required();
            Field(p => p.Nullable).Required();
            Field(p => p.Fixed).Equal("fixed value");
            Field(p => p.OneOf).In(new[] { "one", "two" });
            Field(p => p.SameAsName).Equal(p => p.Name);
            Field(p => p.NotFixed).NotEqual("fixed value");
            Field(p => p.NotOneOf).NotIn(new[] { "one", "two" });
            Field(p => p.NotSameAsName).NotEqual(p => p.Name);
            Field(p => p.MinMaxLength).Length(2, 5);
            Field(p => p.MinLength).MinLength(2);
            Field(p => p.MaxLength).MaxLength(5);
            Field(p => p.Between).Between(2, 3);
            Field(p => p.LessThan).LessThan(3);
            Field(p => p.LessThanOrEqual).LessThanOrEqual(3);
            Field(p => p.GreaterThan).GreaterThan(3);
            Field(p => p.GreaterThanOrEqual).GreaterThanOrEqual(3);
            Field(p => p.RegexString).Matches("^\\w{2,3}$");
            Field(p => p.RegexInstance).Matches(new Regex("^\\w{2,3}$"));
            Field(p => p.MustField).Must(x => x == "x");
            Field(p => p.MustValueField).Must((ctx, x) => x == ctx.MustField);
            Field(p => p.MustFieldAsync).Must(x => Task.FromResult(x == "x"));
            Field(p => p.MustValueFieldAsync).Must((ctx, x) => Task.FromResult(x == ctx.MustFieldAsync));
            Field(p => p.Email).Email();
            Field(p => p.Enum).Enum();
        }
    }
}