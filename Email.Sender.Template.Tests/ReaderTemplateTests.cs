using Email.Sender.Template.Resources;
using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace Email.Sender.Template.Tests
{
    public class ReaderTemplateTests
    {
        [Fact]
        public void Read_Should_Returns_StringContent()
        {
            ReaderTemplate.Read("TemplateDefault.html").Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Read_Should_Throws_FileLoadException()
        {
            Action action = () => ReaderTemplate.Read("ArquivoInexistent.html");
            action.Invoking(self => self()).Should().Throw<FileNotFoundException>();
        }
    }
}