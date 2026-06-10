using System.Net.Mail;
using System.Threading.Tasks;
using Annium.Net.Mail;
using Annium.Net.Mail.Testing;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Mail.Tests;

/// <summary>
/// Tests for <see cref="TestEmailService"/> that do not require DI — direct instantiation.
/// </summary>
public class TestEmailServiceDirectTests
{
    /// <summary>
    /// A freshly constructed <see cref="TestEmailService"/> exposes an empty Emails collection
    /// before any call to SendAsync.
    /// </summary>
    [Fact]
    public void Emails_Initially_IsEmpty()
    {
        var svc = new TestEmailService();

        svc.Emails.IsEmpty();
    }

    /// <summary>
    /// After calling SendAsync, the email is captured in the Emails collection with the correct
    /// Template name and MailMessage reference.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_CapturesEmail_AddsToEmails()
    {
        // arrange
        var svc = new TestEmailService();
        using var message = new MailMessage("from@example.com", "to@example.com", "subject", "");
        var data = new { Name = "Alice" };

        // act
        var result = await svc.SendAsync(message, "welcome", data);

        // assert — result is successful
        result.IsSuccess.IsTrue();

        // assert — exactly one email captured with correct fields
        svc.Emails.Has(1);
        using var enumerator = svc.Emails.GetEnumerator();
        enumerator.MoveNext();
        var email = enumerator.Current;
        email.Template.Is("welcome");
        email.Message.Is(message);
    }

    /// <summary>
    /// Multiple sequential SendAsync calls accumulate all emails in insertion order.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_MultipleCalls_CapturesAllEmails()
    {
        // arrange
        var svc = new TestEmailService();
        using var msg1 = new MailMessage("a@example.com", "b@example.com", "first", "");
        using var msg2 = new MailMessage("c@example.com", "d@example.com", "second", "");

        // act
        await svc.SendAsync(msg1, "tpl-one", new { });
        await svc.SendAsync(msg2, "tpl-two", new { });

        // assert
        svc.Emails.Has(2);
    }
}

/// <summary>
/// Tests for <see cref="TestEmailService"/> registered through the DI container via
/// AddTestEmailService.
/// </summary>
public class TestEmailServiceDiTests : TestBase
{
    /// <summary>
    /// The concrete <see cref="TestEmailService"/> instance registered in the container.
    /// </summary>
    private readonly TestEmailService _testService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestEmailServiceDiTests"/> class.
    /// Registers the <see cref="TestEmailService"/> instance in the container.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public TestEmailServiceDiTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _testService = new TestEmailService();
        Register(c => c.AddTestEmailService(_testService));
    }

    /// <summary>
    /// AddTestEmailService registers the provided <see cref="TestEmailService"/> instance as
    /// <see cref="IEmailService"/>, so resolving <see cref="IEmailService"/> returns the exact
    /// same object.
    /// </summary>
    [Fact]
    public void AddTestEmailService_RegistersAsIEmailService()
    {
        var resolved = Get<IEmailService>();

        resolved.Is(_testService);
    }

    /// <summary>
    /// Emails sent through the <see cref="IEmailService"/> interface resolved from DI are
    /// captured by the underlying <see cref="TestEmailService"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task AddTestEmailService_SendThroughInterface_CapturedByTestService()
    {
        var emailService = Get<IEmailService>();
        using var message = new MailMessage("from@example.com", "to@example.com", "subject", "");

        await emailService.SendAsync(message, "confirmation", new { Code = 42 });

        _testService.Emails.Has(1);
    }
}
