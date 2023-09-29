using System.Reflection;

namespace Annium.Testing.Elements;

public class Test
{
    public string File { get; private set; } = string.Empty;

    public int Line { get; private set; }

    public string FullyQualifiedName { get; private set; }

    public string DisplayName { get; private set; }

    public MethodInfo? Before { get; private set; }

    public MethodInfo Method { get; }

    public MethodInfo? After { get; private set; }

    public bool IsSkipped { get; private set; }

    public Test(MethodInfo method)
    {
        Method = method;
        DisplayName = $"{Method.DeclaringType!.Name}.{Method.Name}";
        FullyQualifiedName = $"{Method.DeclaringType.FullName}.{Method.Name}";
        SetupBefore();
        SetupAfter();
        SetupSkipped();
        SetupLocation();
    }

    public override string ToString() => Method.ToString()!;

    private void SetupSkipped()
    {
        var attribute = Method.GetCustomAttribute<SkipAttribute>();
        if (attribute != null)
        {
            IsSkipped = true;
            UpdateLine(attribute.Line);
        }
    }

    private void SetupBefore()
    {
        var attribute = Method.GetCustomAttribute<BeforeAttribute>();
        if (attribute != null)
        {
            Before = Method.DeclaringType!.GetMethod(attribute.SetUpName)!;
            UpdateLine(attribute.Line);
        }
    }

    private void SetupAfter()
    {
        var attribute = Method.GetCustomAttribute<AfterAttribute>();
        if (attribute != null)
        {
            After = Method.DeclaringType!.GetMethod(attribute.TearDownName)!;
            UpdateLine(attribute.Line);
        }
    }

    private void SetupLocation()
    {
        var attribute = Method.GetCustomAttribute<FactAttribute>();
        File = attribute!.File;
        UpdateLine(attribute.Line);
    }

    ///checking line on each attribute to find best match.
    ///generally, next string will be the one with method
    private void UpdateLine(int line) => Line = line + 1 > Line ? line + 1 : Line;
}