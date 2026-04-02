namespace TheLightStore.Application.Interfaces.Infrastructures;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}
