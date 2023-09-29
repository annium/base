using System.Net.Mail;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Net.Mail;

public interface IEmailService
{
    Task<IBooleanResult> SendAsync<T>(MailMessage message, string template, T data) where T : notnull;
}