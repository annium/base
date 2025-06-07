using System.Net.Mail;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Net.Mail;

/// <summary>
/// Service for sending emails with template support
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email using a template with data
    /// </summary>
    /// <typeparam name="T">The type of the template data</typeparam>
    /// <param name="message">The mail message</param>
    /// <param name="template">The template name</param>
    /// <param name="data">The template data</param>
    /// <returns>A result indicating success or failure</returns>
    Task<IBooleanResult> SendAsync<T>(MailMessage message, string template, T data)
        where T : notnull;
}
