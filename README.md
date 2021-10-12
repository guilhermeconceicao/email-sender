# About this project
A service for email sending using SendGrid, Azure Service Bus, Azure Blob Storage, Serilog and .Net Core

## How?
This microservice has a service bus consumer (broker) that needs the following message:

```
    public class SendEmailMessage
    {
        public string[] Recipients { get; set; } //required
        public string Subject { get; set; } //required
        public string HtmlContentBase64 { get; set; } //required
        public IEnumerable<AttachmentEmail> Attachments { get; set; } //not required
    }
```
Properties explanation:

- `Recipients`: Email recipients list.
- `Subject`: The subject.
- `HtmlContentBase64`: The email content must be HTML assembled. Send the HTML in base 64 format.
- `Attachments`: The attachments. The Azure Blob Storage infos for recover the file + fileName (with the extension)
```
    public class AttachmentEmail : IBlob
    {
        public string FileName { get; set; } //attachment.pdf (do not forget the extension, .pdf, .xml, etc)

        //Infos for recover the file on Azure Blob Storage
        public Guid BlobId { get; set; } 
        public string Owner { get; set; }
        public string Parent { get; set; }
        public string Member { get; set; }
        public string MimeType { get; set; }
    }
```
## Email.Sender.Template
An HTML example

```
TemplateDefaultCreator templateDefault = new TemplateDefaultCreator();
templateDefault.ReplaceTitle("Email title");
templateDefault.ReplaceRecipient("Email recipient");
templateDefault.ReplaceMessage("Email content");
```