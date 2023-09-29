using System.Reflection;
using Annium.Testing.Elements;
using MsTestOutcome = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome;
using MsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Annium.Testing.TestAdapter;

public class TestResultConverter
{
    private readonly TestConverter _testConverter;

    public TestResultConverter(TestConverter testConverter)
    {
        _testConverter = testConverter;
    }

    public MsTestResult Convert(Assembly assembly, Test test, TestResult testResult)
    {
        var result = new MsTestResult(_testConverter.Convert(assembly, test))
        {
            Outcome = (MsTestOutcome)testResult.Outcome,
            ErrorMessage = testResult.Failure?.Message,
            ErrorStackTrace = testResult.Failure?.StackTrace,
            DisplayName = test.DisplayName,
            Duration = testResult.ExecutionDuration,
            StartTime = testResult.ExecutionStart,
            EndTime = testResult.ExecutionEnd
        };

        return result;
    }
}