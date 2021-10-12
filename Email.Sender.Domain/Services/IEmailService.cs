using Email.Sender.Domain.Models;
using System.Threading.Tasks;

namespace Email.Sender.Domain.Services
{
    public interface IEmailService
    {
        Task SendEmail(SendEmailMessage message);
    }
}