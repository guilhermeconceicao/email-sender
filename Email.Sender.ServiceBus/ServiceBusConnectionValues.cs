using Microsoft.Azure.ServiceBus;
using System;

namespace Email.Sender.ServiceBus
{
    public class ServiceBusConnectionValues
    {
        private readonly string connectionString;

        public ServiceBusConnectionValues(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public ServiceBusConnectionStringBuilder CreateConnectionString(string queueName)
        {
            EnsureConnect(queueName);
            ServiceBusConnectionStringBuilder connection = new ServiceBusConnectionStringBuilder(connectionString)
            {
                EntityPath = queueName
            };

            return connection;
        }

        private void EnsureConnect(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException($"'{nameof(queueName)}' cannot be null or whitespace", nameof(queueName));

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("No connection string was provided.");
        }
    }
}