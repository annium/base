using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Annium.Net.Mail;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Mail.Tests;

/// <summary>
/// Tests for the email service pre-send template-loading behaviour when the templates directory
/// does not exist. No real SMTP server is required — the error is raised before any connection.
/// </summary>
public class EmailService_MissingDirectory_Tests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService_MissingDirectory_Tests"/> class.
    /// The templates directory is set to a path that is guaranteed not to exist.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public EmailService_MissingDirectory_Tests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        var missingDir = Path.Combine(Path.GetTempPath(), $"annium-mail-tests-missing-{Guid.NewGuid():N}");
        Register(c =>
            c.AddEmailService(cfg =>
            {
                cfg.TemplatesDirectory = missingDir;
                cfg.Host = "localhost";
                cfg.Port = 25;
            })
        );
    }

    /// <summary>
    /// When the configured templates directory does not exist, SendAsync must throw
    /// <see cref="DirectoryNotFoundException"/> before attempting any SMTP operation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_TemplatesDirectoryMissing_ThrowsDirectoryNotFoundException()
    {
        var service = Get<IEmailService>();
        using var message = new MailMessage("from@example.com", "to@example.com", "subject", "");

        await Wrap.It(async () => await service.SendAsync(message, "welcome", new { Name = "x" }))
            .ThrowsAsync<DirectoryNotFoundException>();
    }
}

/// <summary>
/// Tests for the email service pre-send template-loading behaviour when the templates directory
/// exists but does not contain the requested template file.
/// </summary>
public class EmailService_MissingTemplate_Tests : TestBase, IDisposable
{
    /// <summary>
    /// The temporary directory created for this test instance.
    /// </summary>
    private readonly DirectoryInfo _tempDir;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService_MissingTemplate_Tests"/> class.
    /// Creates a temporary directory that exists but contains no .html files, then registers
    /// the email service pointed at that directory.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public EmailService_MissingTemplate_Tests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _tempDir = Directory.CreateTempSubdirectory("annium-mail-tests-");
        Register(c =>
            c.AddEmailService(cfg =>
            {
                cfg.TemplatesDirectory = _tempDir.FullName;
                cfg.Host = "localhost";
                cfg.Port = 25;
            })
        );
    }

    /// <summary>
    /// Deletes the temporary directory after each test.
    /// </summary>
    public void Dispose()
    {
        _tempDir.Delete(recursive: true);
    }

    /// <summary>
    /// When the templates directory exists but the requested template file is absent,
    /// SendAsync must throw <see cref="FileNotFoundException"/> before attempting any SMTP operation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendAsync_TemplateFileMissing_ThrowsFileNotFoundException()
    {
        var service = Get<IEmailService>();
        using var message = new MailMessage("from@example.com", "to@example.com", "subject", "");

        await Wrap.It(async () => await service.SendAsync(message, "nonexistent-template", new { Name = "x" }))
            .ThrowsAsync<FileNotFoundException>();
    }
}
