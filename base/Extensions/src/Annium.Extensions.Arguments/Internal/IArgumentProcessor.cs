namespace Annium.Extensions.Arguments.Internal;

internal interface IArgumentProcessor
{
    RawConfiguration Compose(string[] args);
}