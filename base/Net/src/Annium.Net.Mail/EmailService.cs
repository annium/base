using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Scriban;
using Scriban.Runtime;

namespace Annium.Net.Mail;

internal class EmailService : IEmailService
{
    private readonly Configuration _cfg;

    public EmailService(
        Configuration cfg
    )
    {
        _cfg = cfg;
    }

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