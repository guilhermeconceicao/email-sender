using System.IO;
using System.Reflection;

namespace Email.Sender.Template.Resources
{
    internal static class ReaderTemplate
    {
        public static string Read(string templateName)
        {
            return GetResourceFileContentAsString(templateName);
        }

        public static string GetResourceFileContentAsString(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Email.Sender.Template.Resources." + fileName;
            Stream stream = null;
            string resource = null;

            try
            {
                stream = assembly.GetManifestResourceStream(resourceName);

                if (stream == null)
                    throw new FileNotFoundException($"Arquivo {fileName} não encontrado.");

                using (StreamReader reader = new StreamReader(stream))
                {
                    resource = reader.ReadToEnd();
                }
                return resource;
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }
        }
    }
}