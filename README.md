# Introduction 

##About this project
This microservice has a service bus consumer (broker) that needs the following message:

```
    public class SendEmailMessage
    {
        public string RecipientsSplitBySemicolon { get; set; } //required
        public string Subject { get; set; } //required
        public string HtmlContentBase64 { get; set; } //required
        public IEnumerable<AttachmentEmail> Attachments { get; set; } //not required
    }
```
Properties explanation:

- `RecipientsSplitBySemicolon`: Email recipients. If you want more than one recipient, split the recipients by semicolon. E.g.:
```
guilherme@gmail.com (one recipient)
OR
guilherme@gmail.com; john@hotmail.com; peter@outlook.com (multiple recipients)

```
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
# Email.Sender.Template
An HTML example

```
TemplateDefaultCreator templateDefault = new TemplateDefaultCreator();
templateDefault.ReplaceTitle("Email title");
templateDefault.ReplaceRecipient("Email recipient");
templateDefault.ReplaceMessage("Email content");
```