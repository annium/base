using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Scriban;
using Scriban.Runtime;

namespace Annium.Net.Mail;

/// <summary>
/// Implementation of email service that sends emails via SMTP using Scriban templates
/// </summary>
internal class EmailService : IEmailService
{
    /// <summary>
    /// The email service configuration
    /// </summary>
    private readonly Configuration _cfg;

    /// <summary>
    /// Initializes a new instance of the EmailService class
    /// </summary>
    /// <param name="cfg">The email service configuration</param>
    public EmailService(Configuration cfg)
    {
        _cfg = cfg;
    }

    /// <summary>
    /// Sends an email using a template with data
    /// </summary>
    /// <typeparam name="T">The type of the template data</typeparam>
    /// <param name="message">The mail message</param>
    /// <param name="template">The template name (without .html extension)</param>
    /// <param name="data">The template data to be injected into the template</param>
    /// <returns>A result indicating success or failure of the email sending operation</returns>
    public async Task<IBooleanResult> SendAsync<T>(MailMessage message, string template, T data)
        where T : notnull
    {
        using var client = GetClient();
        message.Body = LoadBody(template, data);
        message.IsBodyHtml = true;

        try
        {
            await client.SendMailAsync(message);

            return Result.Success();
        }
        catch (SmtpException ex)
        {
            return Result.Failure().Error(ex.Message);
        }
    }

    /// <summary>
    /// Creates and configures an SMTP client using the service configuration
    /// </summary>
    /// <returns>A configured SMTP client</returns>
    private SmtpClient GetClient()
    {
        return new(_cfg.Host, _cfg.Port)
        {
            EnableSsl = _cfg.UseSsl,
            UseDefaultCredentials = false,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(_cfg.User, _cfg.Password),
        };
    }

    /// <summary>
    /// Loads and renders an email template with the provided data using Scriban templating engine
    /// </summary>
    /// <typeparam name="T">The type of the template data</typeparam>
    /// <param name="template">The template name (without .html extension)</param>
    /// <param name="data">The data to be injected into the template</param>
    /// <returns>The rendered email body as HTML</returns>
    private string LoadBody<T>(string template, T data)
    {
        var templatesDirectory = Path.GetFullPath(_cfg.TemplatesDirectory);
        if (!Directory.Exists(templatesDirectory))
            throw new DirectoryNotFoundException($"Email templates directory {templatesDirectory} missing");

        var templateFile = Path.Combine(templatesDirectory, $"{template}.html");
        if (!File.Exists(templateFile))
            throw new FileNotFoundException($"Email template {templateFile} missing");

        var scriptObject = new ScriptObject();
        scriptObject.Import(data);
        var ctx = new TemplateContext();
        ctx.PushGlobal(scriptObject);

        using var reader = File.OpenText(templateFile);
        var tpl = Template.Parse(reader.ReadToEnd());
        var result = tpl.Render(ctx);

        return result;
    }
}
