using System;
using System.Net;
using System.Net.Mail;
using TheLightStore.Application.Interfaces.Infrastructures;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Infrastructure.Services;

public class EmailSenderService : IEmailSenderService
{
    public async Task<ResponseResult> SendMailAsync(string fromEmail, string fromPassWord, string toEmail, string sendMailTitle, string sendMailBody)
    {
        try
        {
            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(fromEmail, fromPassWord);

                var mailMessage = new MailMessage(fromEmail, toEmail)
                {
                    Subject = sendMailTitle,
                    Body = sendMailBody,
                    IsBodyHtml = true
                };

                await smtp.SendMailAsync(mailMessage);
                mailMessage.Dispose();

                return new ResponseResult { IsSuccess = true, Message = "Email sent successfully" };
            }
        }
        catch (Exception ex)
        {
            return new ResponseResult { IsSuccess = false, Message = $"Failed to send email: {ex.Message}" };
        }
    }

    public async Task<ResponseResult> SendMailAsyncWithSmtp(string fromEmail, string toEmail, string sendMailTitle, string sendMailBody, SmtpClient smtp)
    {
        try
        {
            var mailMessage = new MailMessage(fromEmail, toEmail)
            {
                Subject = sendMailTitle,
                Body = sendMailBody,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(mailMessage);
            mailMessage.Dispose();

            return new ResponseResult { IsSuccess = true, Message = "Email sent successfully" };
        }
        catch (Exception ex)
        {
            return new ResponseResult { IsSuccess = false, Message = $"Failed to send email: {ex.Message}" };
        }
    }
}
