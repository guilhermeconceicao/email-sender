using System.Collections.Generic;

namespace Email.Sender.Domain.Models
{
    public class SendEmailMessage
    {
        public string RecipientsSplitBySemicolon { get; set; }
        public string Subject { get; set; }
        public string HtmlContentBase64 { get; set; }
        public IEnumerable<AttachmentEmail> Attachments { get; set; }
    }
}