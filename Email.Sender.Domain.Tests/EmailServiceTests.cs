using Email.Sender.BlobStorage;
using Email.Sender.Domain.Models;
using Email.Sender.Domain.Services;
using FluentAssertions;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xunit;

namespace Email.Sender.Domain.Tests
{
    public class EmailServiceTests
    {
        private readonly IEmailService emailService;
        private readonly Mock<ISendGridClient> sendGridClient;
        private readonly Mock<IBlobStorage> blobStorage;

        public EmailServiceTests()
        {
            sendGridClient = new Mock<ISendGridClient>();
            blobStorage = new Mock<IBlobStorage>();

            emailService = new EmailService(sendGridClient.Object, blobStorage.Object);
        }

        [Theory]
        [InlineData("   ", "Subject", "Html")]
        [InlineData("Address", "    ", "Html")]
        [InlineData("Address", "Subject", "     ")]
        public void ValidMessage_MessageWithWhiteSpace_ThrowsArgumentException(string toAddress, string subject, string html)
        {
            //Arrange
            SendEmailMessage message = new SendEmailMessage
            {
                RecipientsSplitBySemicolon = toAddress,
                Subject = subject,
                HtmlContentBase64 = html
            };

            //Act + Assert
            emailService.Invoking(s => s.SendEmail(message).Wait()).Should().Throw<ArgumentException>().WithMessage("Invalid message (Parameter 'SendEmailMessage')");

            sendGridClient.Verify(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Theory]
        [InlineData(null, "Subject", "Html")]
        [InlineData("Address", null, "Html")]
        [InlineData("Address", "Subject", null)]
        public void ValidMessage_MessageWithNull_ThrowsArgumentException(string toAddress, string subject, string html)
        {
            //Arrange
            SendEmailMessage message = new SendEmailMessage
            {
                RecipientsSplitBySemicolon = toAddress,
                Subject = subject,
                HtmlContentBase64 = html
            };

            //Act + Assert
            emailService.Invoking(s => s.SendEmail(message).Wait()).Should().Throw<ArgumentException>().WithMessage("Invalid message (Parameter 'SendEmailMessage')");

            sendGridClient.Verify(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public void ValidMessage_MessageWithAttachmentNull_ThrowsArgumentException()
        {
            //Arrange
            Guid blobId = Guid.NewGuid();
            SendEmailMessage message = new SendEmailMessage
            {
                RecipientsSplitBySemicolon = "Address",
                Subject = "Subject",
                HtmlContentBase64 = GetHtmlBase64(),
                Attachments = new List<AttachmentEmail>
                {
                    new AttachmentEmail
                    {
                        FileName = "attachmentEmail.pdf"
                    }
                }
            };
            Stream stream = null;
            blobStorage.Setup(x => x.GetStreamAsync(It.IsAny<IBlob>())).ReturnsAsync(stream);

            //Act + Assert
            emailService.Invoking(s => s.SendEmail(message).Wait()).Should().Throw<ArgumentException>().WithMessage("Invalid attachment (Parameter 'attachmentEmail.pdf')");

            sendGridClient.Verify(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public void ValidMessage_InvokesSendEmailAsync_OnlyOneRecipientAndAttachment()
        {
            //Arrange
            SendEmailMessage message = new SendEmailMessage
            {
                RecipientsSplitBySemicolon = "addres@email.com",
                Subject = "Subject",
                HtmlContentBase64 = GetHtmlBase64(),
                Attachments = new List<AttachmentEmail>
                {
                    new AttachmentEmail
                    {
                        FileName = "attachmentEmail.pdf",
                        MimeType = "application/pdf"
                    }
                }
            };

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write("Teste");
            streamWriter.Flush();
            var stream = streamWriter.BaseStream;
            blobStorage.Setup(x => x.GetStreamAsync(It.IsAny<IBlob>())).ReturnsAsync(stream);

            //Act
            emailService.SendEmail(message).Wait();

            //Assert
            sendGridClient.Verify(x => x.SendEmailAsync(It.Is<SendGridMessage>(x => x.Personalizations[0].Tos[0].Email == "addres@email.com" &&
                                                                                    x.Personalizations[0].Subject == "Subject" &&
                                                                                    x.Contents[0].Type == "text/html" &&
                                                                                    !string.IsNullOrWhiteSpace(x.Contents[0].Value) &&
                                                                                    x.Attachments[0].Filename == "attachmentEmail.pdf" &&
                                                                                    x.Attachments[0].Type == "application/pdf" &&
                                                                                    x.Attachments[0].Content == "VGVzdGU="),
                                                                                    It.IsAny<CancellationToken>()),
                                                                                    Times.Once());
        }

        [Fact]
        public void ValidMessage_InvokesSendEmailAsync_MultipleRecipientsAndAttachments()
        {
            //Arrange
            SendEmailMessage message = new SendEmailMessage
            {
                RecipientsSplitBySemicolon = "addres@email.com; teste@hotmail.com",
                Subject = "Subject",
                HtmlContentBase64 = GetHtmlBase64(),
                Attachments = new List<AttachmentEmail>
                {
                    new AttachmentEmail
                    {
                        FileName = "attachmentEmail.pdf",
                        MimeType = "application/pdf"
                    },
                    new AttachmentEmail
                    {
                        FileName = "teste.pdf",
                        MimeType = "text/xml"
                    }
                }
            };

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write("Teste");
            streamWriter.Flush();
            var stream = streamWriter.BaseStream;
            blobStorage.Setup(x => x.GetStreamAsync(It.IsAny<IBlob>())).ReturnsAsync(stream);

            //Act
            emailService.SendEmail(message).Wait();

            //Assert
            sendGridClient.Verify(x => x.SendEmailAsync(It.Is<SendGridMessage>(x => x.Personalizations[0].Tos[0].Email == "addres@email.com" &&
                                                                                    x.Personalizations[0].Tos[1].Email == "teste@hotmail.com" &&
                                                                                    x.Personalizations[0].Subject == "Subject" &&
                                                                                    x.Contents[0].Type == "text/html" &&
                                                                                    !string.IsNullOrWhiteSpace(x.Contents[0].Value) &&
                                                                                    x.Attachments[0].Filename == "attachmentEmail.pdf" &&
                                                                                    x.Attachments[0].Type == "application/pdf" &&
                                                                                    x.Attachments[0].Content == "VGVzdGU=" &&
                                                                                    x.Attachments[1].Filename == "teste.pdf" &&
                                                                                    x.Attachments[1].Type == "text/xml" &&
                                                                                    x.Attachments[1].Content == "VGVzdGU="),
                                                                                    It.IsAny<CancellationToken>()),
                                                                                    Times.Once());
        }

        private string GetHtmlBase64()
        {
            return "PCFET0NUWVBFIGh0bWwgUFVCTElDICItLy9XM0MvL0RURCBYSFRNTCAxLjAgVHJhbnNpdGlvbmFsLy9FTiIgImh0dHBzOi8vd3d3LnczLm9yZy9UUi94aHRtbDEvRFREL3hodG1sMS10cmFuc2l0aW9uYWwuZHRkIj4KPGh0bWwgbGFuZz0icHQtYnIiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hodG1sIj4KPGhlYWQ+CiAgICA8bWV0YSBodHRwLWVxdWl2PSJDb250ZW50LVR5cGUiIGNvbnRlbnQ9InRleHQvaHRtbDsgY2hhcnNldD1VVEYtOCIgLz4KICAgIDxtZXRhIGh0dHAtZXF1aXY9IlgtVUEtQ29tcGF0aWJsZSIgY29udGVudD0iSUU9ZWRnZSIgLz4KICAgIDxtZXRhIG5hbWU9InZpZXdwb3J0IiBjb250ZW50PSJ3aWR0aD1kZXZpY2Utd2lkdGgsIGluaXRpYWwtc2NhbGU9MS4wICIgLz4KICAgIDxsaW5rIGhyZWY9Imh0dHBzOi8vZm9udHMuZ29vZ2xlYXBpcy5jb20vY3NzP2ZhbWlseT1BcmlhbHxOdW5pdG8rU2Fuczp3Z2h0QDYwMCZmYW1pbHk9TnVuaXRvOndnaHRANzAwIiByZWw9InN0eWxlc2hlZXQiPgogICAgPHRpdGxlPkVtYWlsIHBlZGlkbyBBbWJldjwvdGl0bGU+CjwvaGVhZD4KPGJvZHkgc3R5bGU9Im1hcmdpbjogMCBhdXRvOyBib3JkZXI6IG5vbmU7Ij4KICAgIDx0YWJsZSBhbGlnbj0iY2VudGVyIgogICAgICAgICAgIHN0eWxlPSJib3JkZXI6IDFweCBzb2xpZCAjY2NjOwogICAgICAgIC13ZWJraXQtYm9yZGVyLXJhZGl1czogMjBweDsKICAgICAgICBib3JkZXItcmFkaXVzOiAyMHB4OwogICAgICAgIGJvcmRlci1ib3R0b206IDE1cHggc29saWQgIzAwNjdmZDsKICAgICAgICBwYWRkaW5nOiAxNXB4OwogICAgICAgIG1hcmdpbi10b3A6IDUwcHg7IgogICAgICAgICAgIGNlbGxwYWRkaW5nPSIwIiBjZWxsc3BhY2luZz0iMCIgd2lkdGg9IjYwMHB4Ij4KICAgICAgICA8Y2FwdGlvbj48L2NhcHRpb24+CiAgICAgICAgPHRyPgogICAgICAgICAgICA8dGggaWQ9ImhlYWRlclZhemlvIj48L3RoPgogICAgICAgIDwvdHI+CiAgICAgICAgPHRyPgogICAgICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICA8dGFibGUgYm9yZGVyPSIwIiBjZWxscGFkZGluZz0iMCIgY2VsbHNwYWNpbmc9IjAiIHdpZHRoPSI2MDBweCI+CiAgICAgICAgICAgICAgICAgICAgPGNhcHRpb24+PC9jYXB0aW9uPgogICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRoIGlkPSJsaW5rSW1hZ2UiIGFsaWduPSJjZW50ZXIiIHN0eWxlPSJwYWRkaW5nOiAyMHB4OyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICA8aW1nIHNyYz0iaHR0cHM6Ly9nZXN0b3JzdGdhY2NkZXYuejEzLndlYi5jb3JlLndpbmRvd3MubmV0L2xvZ28tZ2VzdG9yLWJsdWUucG5nIiBhbHQ9Ikdlc3RvciIgd2lkdGg9IjI1MCIgaGVpZ2h0PSI3MCIgc3R5bGU9ImRpc3BsYXk6IGJsb2NrOyIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICA8dHIgLz4KCiAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICA8dGQgc3R5bGU9InBhZGRpbmc6IDIwcHg7Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxoMSBzdHlsZT0ibWFyZ2luOiAwOyBmb250LXNpemU6IDI1cHg7IHRleHQtYWxpZ246IGNlbnRlcjsgZm9udC1mYW1pbHk6ICdOdW5pdG8gU2FucycsIHNhbnMtc2VyaWY7Ij5AVGl0bGU8L2gxPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgIDx0ciAvPgoKICAgICAgICAgICAgICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBzdHlsZT0icGFkZGluZzogNXB4IDIwcHggNXB4IDVweDsiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgPHAgc3R5bGU9ImZvbnQtc2l6ZTogMTZweDsgZm9udC1mYW1pbHk6ICdOdW5pdG8gU2FucycsIHNhbnMtc2VyaWY7Ij5PbMOhIEBSZWNpcGllbnQsPC9wPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgPHAgc3R5bGU9ImZvbnQtc2l6ZTogMTZweDsgZm9udC1mYW1pbHk6ICdOdW5pdG8gU2FucycsIHNhbnMtc2VyaWY7Ij5ATWVzc2FnZTwvcD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICA8dHIgLz4KCiAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICA8dGQgc3R5bGU9InBhZGRpbmc6IDVweCAyMHB4IDVweCA1cHg7IGxpbmUtaGVpZ2h0OiAycHg7Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxwIHN0eWxlPSJmb250LXNpemU6IDE2cHg7IGZvbnQtZmFtaWx5OiAnTnVuaXRvIFNhbnMnLCBzYW5zLXNlcmlmOyI+QXRlbmNpb3NhbWVudGUsPC9wPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxwIHN0eWxlPSJmb250LXNpemU6IDE2cHg7IGZvbnQtZmFtaWx5OiAnTnVuaXRvIFNhbnMnLCBzYW5zLXNlcmlmOyI+RXF1aXBlIEdlc3RvciBBbWJldi48L3A+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgPHRyIC8+CiAgICAgICAgICAgICAgICA8L3RhYmxlPgogICAgICAgICAgICA8L3RkPgogICAgICAgIDwvdHI+CiAgICA8L3RhYmxlPgo8L2JvZHk+CjwvaHRtbD4=";
        }
    }
}