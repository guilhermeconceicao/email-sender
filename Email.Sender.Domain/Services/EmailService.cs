using Email.Sender.BlobStorage;
using Email.Sender.CrossCutting.Envs;
using Email.Sender.Domain.Extensions;
using Email.Sender.Domain.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Email.Sender.Domain.Services
{
    public class EmailService : IEmailService
    {
        private static readonly ILogger logger = Log.Logger.ForContext<EmailService>();
        private readonly ISendGridClient sendGridClient;
        private readonly IBlobStorage blobStorage;

        public EmailService(
            ISendGridClient sendGridClient,
            IBlobStorage blobStorage)
        {
            this.sendGridClient = sendGridClient;
            this.blobStorage = blobStorage;
        }

        public async Task SendEmail(SendEmailMessage message)
        {
            ValidateMessage(message);
            var sendGridMessage = await CreateSendGridMessage(message);
            var response = await sendGridClient.SendEmailAsync(sendGridMessage);
            logger.Debug("StatusCode {StatusCode} - Email sent. FromAddress: {FromAddress}. FromName: {FromName} Tos: {RecipientsSplitBySemicolon}",
                response?.StatusCode.ToString(), EnvironmentVars.EmailFromAddress, EnvironmentVars.EmailFromName, message.RecipientsSplitBySemicolon);
        }

        private void ValidateMessage(SendEmailMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.RecipientsSplitBySemicolon)
                || string.IsNullOrWhiteSpace(message.Subject)
                || string.IsNullOrWhiteSpace(message.HtmlContentBase64))
            {
                throw new ArgumentException("Invalid message", typeof(SendEmailMessage).Name);
            }
        }

        private async Task<SendGridMessage> CreateSendGridMessage(SendEmailMessage message)
        {
            var sendGridMessage = new SendGridMessage();
            sendGridMessage.SetFrom(new EmailAddress(EnvironmentVars.EmailFromAddress, EnvironmentVars.EmailFromName));
            sendGridMessage.AddTos(GetRecipientsEmails(message.RecipientsSplitBySemicolon));
            sendGridMessage.SetSubject(message.Subject);
            sendGridMessage.AddContent(MimeType.Html, GetHtmlContent(message.HtmlContentBase64));
            if (message.Attachments?.Any() ?? false)
                sendGridMessage.AddAttachments(await GetAttachmentsEmail(message));

            return sendGridMessage;
        }

        private List<EmailAddress> GetRecipientsEmails(string recipientsSplitBySemicolon)
        {
            string[] recipients = recipientsSplitBySemicolon.Split(';');

            var tos = new List<EmailAddress>();
            foreach (string recipientEmail in recipients)
                tos.Add(new EmailAddress(recipientEmail.RemoveWhiteSpaces()));

            return tos;
        }

        private string GetHtmlContent(string htmlContentBase64)
        {
            byte[] data = Convert.FromBase64String(htmlContentBase64);
            return Encoding.UTF8.GetString(data);
        }

        private async Task<List<Attachment>> GetAttachmentsEmail(SendEmailMessage message)
        {
            var attachments = new List<Attachment>();
            foreach (AttachmentEmail attachment in message.Attachments)
            {
                Stream attachmentBlob = await GetAttachmentBlob(attachment);
                attachments.Add(new Attachment
                {
                    Content = ConvertToBase64(attachmentBlob),
                    Filename = attachment.FileName,
                    Type = attachment.MimeType
                });
            }
            return attachments;
        }

        private async Task<Stream> GetAttachmentBlob(AttachmentEmail attachment)
        {
            Stream attachmentBlob = await blobStorage.GetStreamAsync(attachment);
            if (attachmentBlob == null)
                throw new ArgumentException("Invalid attachment", attachment.FileName);

            return attachmentBlob;
        }

        private string ConvertToBase64(Stream stream)
        {
            byte[] bytes = new byte[(int)stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, (int)stream.Length);
            return Convert.ToBase64String(bytes);
        }
    }
}