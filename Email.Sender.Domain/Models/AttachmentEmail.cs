using Email.Sender.BlobStorage;
using System;

namespace Email.Sender.Domain.Models
{
    public class AttachmentEmail : IBlob
    {
        public string FileName { get; set; }

        public Guid BlobId { get; set; }
        public string Owner { get; set; }
        public string Parent { get; set; }
        public string Member { get; set; }
        public string MimeType { get; set; }
    }
}