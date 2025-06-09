using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Net.Mail;

/// <summary>
/// Test implementation of email service that captures sent emails instead of actually sending them
/// </summary>
public class TestEmailService : IEmailService
{
    /// <summary>
    /// Gets a read-only collection of all captured emails
    /// </summary>
    public IReadOnlyCollection<TestEmail> Emails => _emails;

    /// <summary>
    /// Internal list to store captured test emails
    /// </summary>
    private readonly List<TestEmail> _emails = new();

    /// <summary>
    /// Captures an email instead of sending it, useful for testing scenarios
    /// </summary>
    /// <typeparam name="T">The type of the template data</typeparam>
    /// <param name="message">The mail message</param>
    /// <param name="template">The template name</param>
    /// <param name="data">The template data</param>
    /// <returns>A successful result indicating the email was captured</returns>
    public Task<IBooleanResult> SendAsync<T>(MailMessage message, string template, T data)
        where T : notnull
    {
        _emails.Add(new TestEmail(message, template, data));

        return Task.FromResult(Result.Success());
    }

    /// <summary>
    /// Represents a captured email for testing purposes
    /// </summary>
    /// <param name="Message">The mail message that would have been sent</param>
    /// <param name="Template">The template name that was used</param>
    /// <param name="Data">The data that was provided for template rendering</param>
    public record TestEmail(MailMessage Message, string Template, object Data);
}
