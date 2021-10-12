using FluentAssertions;
using Xunit;

namespace Email.Sender.Template.Tests
{
    public class TemplateDefaultReplacementTests
    {
        [Fact]
        public void Replacements_Tests()
        {
            //Arrange
            string titulo = "Titulo do email";
            string destinatario = "Destinatario do email";
            string message = "Message do email";

            TemplateDefaultCreator templateDefault = new TemplateDefaultCreator();
            templateDefault.ReplaceTitle(titulo);
            templateDefault.ReplaceRecipient(destinatario);
            templateDefault.ReplaceMessage(message);

            //Act
            var template = templateDefault.GetTemplate();

            //Assert
            template.Should().Contain(titulo);
            template.Should().Contain(destinatario);
            template.Should().Contain(message);
        }
    }
}