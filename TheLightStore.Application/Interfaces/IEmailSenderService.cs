using System;
using System.Net.Mail;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Application.Interfaces;

public interface IEmailSenderService
{
    Task<ResponseResult> SendMailAsync(string fromEmail, string fromPassWord, string toEmail, string sendMailTitle, string sendMailBody);
    Task<ResponseResult> SendMailAsyncWithSmtp(string fromEmail, string toEmail, string sendMailTitle, string sendMailBody, SmtpClient smtp);
}
