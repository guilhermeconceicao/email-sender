using System;

namespace Email.Sender.BlobStorage
{
    public interface IBlob
    {
        public Guid BlobId { get; set; }

        public string Owner { get; set; }

        public string Parent { get; set; }

        public string Member { get; set; }

        public string MimeType { get; set; }
    }
}