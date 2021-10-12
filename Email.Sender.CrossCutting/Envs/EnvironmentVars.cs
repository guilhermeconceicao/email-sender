using System;

namespace Email.Sender.CrossCutting.Envs
{
    public static class EnvironmentVars
    {
        public static string ServiceBusConnectionString => Environment.GetEnvironmentVariable("SERVICEBUS-CONNECTION-STRING");

        public static string SendEmailQueueName => Environment.GetEnvironmentVariable("SEND-EMAIL-QUEUE-NAME");

        public static string EmailApiKey => Environment.GetEnvironmentVariable("EMAIL-API-KEY");

        public static string EmailFromAddress => Environment.GetEnvironmentVariable("EMAIL-FROM-ADDRESS");

        public static string EmailFromName => Environment.GetEnvironmentVariable("EMAIL-FROM-NAME");

        public static string BlobConnectionString => Environment.GetEnvironmentVariable("BLOB-STORAGE-CONNECTION-STRING");

        public static string BlobContainer => Environment.GetEnvironmentVariable("BLOB-STORAGE-CONTAINER");
    }
}