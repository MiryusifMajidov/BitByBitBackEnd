using BitByBit.Business.DTOs.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.Services.Interfaces
{
    public interface IEmailService
    {
        #region Base Email Methods

        /// <summary>
        /// Send simple text email
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);

        /// <summary>
        /// Send email using EmailDto
        /// </summary>
        Task<bool> SendEmailAsync(EmailDto emailDto);

        /// <summary>
        /// Send HTML email
        /// </summary>
        Task<bool> SendHtmlEmailAsync(string toEmail, string subject, string htmlBody);

        #endregion

        #region Template-based HTML Email Methods

        /// <summary>
        /// Send beautiful HTML confirmation code email
        /// </summary>
        Task<bool> SendConfirmationCodeAsync(string toEmail, string firstName, string confirmationCode);

        /// <summary>
        /// Send beautiful HTML welcome email
        /// </summary>
        Task<bool> SendWelcomeEmailAsync(string toEmail, string firstName);

        /// <summary>
        /// Send beautiful HTML password reset email
        /// </summary>
        Task<bool> SendPasswordResetCodeAsync(string toEmail, string firstName, string resetCode);

        #endregion

        #region Fallback Text Methods

        /// <summary>
        /// Send text-only confirmation code (fallback)
        /// </summary>
        Task<bool> SendConfirmationCodeTextAsync(string toEmail, string firstName, string confirmationCode);

        /// <summary>
        /// Send text-only welcome email (fallback)
        /// </summary>
        Task<bool> SendWelcomeTextAsync(string toEmail, string firstName);

        /// <summary>
        /// Send text-only password reset (fallback)
        /// </summary>
        Task<bool> SendPasswordResetTextAsync(string toEmail, string firstName, string resetCode);

        #endregion

        #region Advanced Email Methods

        /// <summary>
        /// Send custom HTML email with generic template
        /// </summary>
        Task<bool> SendCustomHtmlEmailAsync(string toEmail, string title, string content, string primaryColor = "#667eea");

        /// <summary>
        /// Send bulk emails (newsletters, notifications)
        /// </summary>
        Task<bool> SendBulkEmailAsync(List<string> toEmails, string subject, string htmlBody);

        /// <summary>
        /// Send email with attachment
        /// </summary>
        Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, string attachmentPath, string attachmentName);

        #endregion
    }
}