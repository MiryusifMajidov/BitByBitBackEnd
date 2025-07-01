using BitByBit.Business.DTOs.Email;
using BitByBit.Business.Helpers;
using BitByBit.Business.Services.Interfaces;
using BitByBit.Core.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BitByBit.Business.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        #region Base Email Methods

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.FromEmail, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(EmailDto emailDto)
        {
            if (emailDto.IsHtml)
            {
                return await SendHtmlEmailAsync(emailDto.ToEmail, emailDto.Subject, emailDto.Body);
            }
            return await SendEmailAsync(emailDto.ToEmail, emailDto.Subject, emailDto.Body);
        }

        public async Task<bool> SendHtmlEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.FromEmail, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Template-based HTML Email Methods

       
        public async Task<bool> SendConfirmationCodeAsync(string toEmail, string firstName, string confirmationCode)
        {
            var subject = " Email Təsdiqi - BitByBit";
            var htmlBody = EmailTemplateHelper.GetConfirmationCodeTemplate(firstName, confirmationCode);

            return await SendHtmlEmailAsync(toEmail, subject, htmlBody);
        }

 
        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string firstName)
        {
            var subject = " Xoş gəlmisiniz - BitByBit";
            var htmlBody = EmailTemplateHelper.GetWelcomeTemplate(firstName);

            return await SendHtmlEmailAsync(toEmail, subject, htmlBody);
        }

    
        public async Task<bool> SendPasswordResetCodeAsync(string toEmail, string firstName, string resetCode)
        {
            var subject = "🔑 Şifrə Sıfırlama - BitByBit";
            var htmlBody = EmailTemplateHelper.GetPasswordResetTemplate(firstName, resetCode);

            return await SendHtmlEmailAsync(toEmail, subject, htmlBody);
        }

        #endregion

        #region Fallback Text Methods (Backup)

        public async Task<bool> SendConfirmationCodeTextAsync(string toEmail, string firstName, string confirmationCode)
        {
            var subject = "Email Təsdiqi - BitByBit";
            var body = $@"
Salam {firstName},

BitByBit hesabınızı təsdiqləmək üçün aşağıdakı kodu daxil edin:

Təsdiq Kodu: {confirmationCode}

Bu kod 24 saat müddətində keçərlidir.

Hörmətlə,
BitByBit Team";

            return await SendEmailAsync(toEmail, subject, body);
        }

       
        public async Task<bool> SendWelcomeTextAsync(string toEmail, string firstName)
        {
            var subject = "Xoş gəlmisiniz - BitByBit";
            var body = $@"
Salam {firstName},

BitByBit ailəsinə xoş gəlmisiniz! 

Hesabınız uğurla yaradıldı və artıq platformamızdan istifadə edə bilərsiniz.

Hörmətlə,
BitByBit Team";

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendPasswordResetTextAsync(string toEmail, string firstName, string resetCode)
        {
            var subject = "Şifrə Sıfırlama - BitByBit";
            var body = $@"
Salam {firstName},

Şifrənizi sıfırlamaq üçün aşağıdakı kodu daxil edin:

Sıfırlama Kodu: {resetCode}

Bu kod 30 dəqiqə müddətində keçərlidir.

Əgər siz bu tələbi etməmisinizsə, bu email-i nəzərə almayın.

Hörmətlə,
BitByBit Team";

            return await SendEmailAsync(toEmail, subject, body);
        }

        #endregion

        #region Advanced Email Methods

   
        public async Task<bool> SendCustomHtmlEmailAsync(string toEmail, string title, string content, string primaryColor = "#667eea")
        {
            var htmlBody = EmailTemplateHelper.GetGenericTemplate(title, content, primaryColor);
            return await SendHtmlEmailAsync(toEmail, title, htmlBody);
        }

        public async Task<bool> SendBulkEmailAsync(List<string> toEmails, string subject, string htmlBody)
        {
            var tasks = toEmails.Select(email => SendHtmlEmailAsync(email, subject, htmlBody));
            var results = await Task.WhenAll(tasks);
            return results.All(result => result);
        }

        
        public async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, string attachmentPath, string attachmentName)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };

                
                if (File.Exists(attachmentPath))
                {
                    var attachment = new MimePart()
                    {
                        Content = new MimeContent(File.OpenRead(attachmentPath)),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = attachmentName
                    };
                    bodyBuilder.Attachments.Add(attachment);
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.FromEmail, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}