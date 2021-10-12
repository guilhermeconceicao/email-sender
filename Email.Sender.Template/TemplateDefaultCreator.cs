using Email.Sender.Template.Resources;

namespace Email.Sender.Template
{
    public class TemplateDefaultCreator
    {
        private string _templateContent = ReaderTemplate.Read("TemplateDefault.html");

        public void DefineTemplate(string newTemplate)
        {
            _templateContent = newTemplate;
        }

        public string GetTemplate()
        {
            return _templateContent;
        }
    }
}