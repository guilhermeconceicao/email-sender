using Email.Sender.CrossCutting.Envs;
using Email.Sender.ServiceBus.Extensions;
using Email.Sender.ServiceBus.Models;
using Email.Sender.ServiceBus.Models.Enums;
using Microsoft.Azure.ServiceBus;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Email.Sender.ServiceBus
{
    public class ServiceBusConsumer<TMessage> where TMessage : class
    {
        private static readonly ILogger logger = Log.Logger.ForContext<ServiceBusConsumer<TMessage>>();
        private readonly ServiceBusConnectionStringBuilder connectionString;
        private readonly ActionTypes queueType;
        private readonly string queueName;
        private IQueueClient queueClient;
        private Func<TMessage, Task<ProcessResponse>> ProcessFunction { get; set; }

        public ServiceBusConsumer(ActionTypes queueType)
        {
            this.queueType = queueType;

            queueName = !string.IsNullOrWhiteSpace(EnvironmentVars.SendEmailQueueName)
                ? EnvironmentVars.SendEmailQueueName
                : throw new ArgumentException("Email queue name was null");

            connectionString = new ServiceBusConnectionValues(EnvironmentVars.ServiceBusConnectionString).CreateConnectionString(queueName);
        }

        #region Register

        public void Register(Func<TMessage, Task<ProcessResponse>> messageProcess)
        {
            RefreshQueueClient();
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            ProcessFunction = messageProcess;
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs eventArgs)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine("Service Bus encountered an exception.");
            errorMessage.AppendLine("Exception context for troubleshooting:");
            errorMessage.AppendLine($"- Endpoint: {eventArgs.ExceptionReceivedContext.Endpoint}");
            errorMessage.AppendLine($"- Queue: {eventArgs.ExceptionReceivedContext.EntityPath}");
            errorMessage.AppendLine($"- Executing Action: {eventArgs.ExceptionReceivedContext.Action}");
            logger.Error(eventArgs.Exception, errorMessage.ToString());

            Thread.Sleep(60000);
            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(Message messageBus, CancellationToken token)
        {
            var message = messageBus.Body.Deserialize<TMessage>();
            var response = await ProcessFunction(message);

            switch (response.Status)
            {
                case MessageProcessResponse.Complete:
                    await queueClient.CompleteAsync(messageBus.SystemProperties.LockToken);
                    break;

                case MessageProcessResponse.ReEnqueue:
                    break;

                case MessageProcessResponse.DeadLetter:
                    await DeadLetterAsync(messageBus, response.Message);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid MessageProcessResponse {response}.");
            }
        }

        private async Task DeadLetterAsync(Message messageBus, string message)
        {
            var customProperties = new Dictionary<string, object>
            {
                ["Message"] = message
            };

            await queueClient.DeadLetterAsync(messageBus.SystemProperties.LockToken, customProperties);
        }

        #endregion Register

        public void RefreshQueueClient()
        {
            if (queueClient == null || queueClient.IsClosedOrClosing)
            {
                if (queueType == ActionTypes.DeadLetterQueue)
                    connectionString.EntityPath = $"{ queueName}/$DeadLetterQueue";

                queueClient = new QueueClient(connectionString, ReceiveMode.PeekLock, RetryPolicy.Default);
            }
        }

        public void Deregister()
        {
            queueClient.CloseAsync();
        }
    }
}