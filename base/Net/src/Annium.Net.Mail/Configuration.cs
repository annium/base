namespace Annium.Net.Mail;

/// <summary>
/// Configuration settings for email service
/// </summary>
public class Configuration
{
    /// <summary>
    /// Gets or sets the directory containing email templates
    /// </summary>
    public string TemplatesDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP server host
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP server port
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets whether to use SSL for SMTP connection
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// Gets or sets the SMTP authentication username
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP authentication password
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
