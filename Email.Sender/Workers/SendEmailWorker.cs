using Email.Sender.CrossCutting.Envs;
using Email.Sender.Domain.Models;
using Email.Sender.Domain.Services;
using Email.Sender.ServiceBus;
using Email.Sender.ServiceBus.Models;
using Email.Sender.ServiceBus.Models.Enums;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Email.Sender.Workers
{
    public class SendEmailWorker : BackgroundService
    {
        private static readonly ILogger logger = Log.Logger.ForContext<SendEmailWorker>();
        private ServiceBusConsumer<SendEmailMessage> consumer;
        private readonly IEmailService emailService;

        public SendEmailWorker(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        public async Task<ProcessResponse> ProcessMessageAsync(SendEmailMessage message)
        {
            try
            {
                logger.Debug("Message received from queue {QueueName}", EnvironmentVars.SendEmailQueueName);
                await emailService.SendEmail(message);
                return new ProcessResponse(MessageProcessResponse.Complete);
            }
            catch (ArgumentException ex)
            {
                logger.Error(ex, "Error message: {Message}", ex.Message);
                return new ProcessResponse(MessageProcessResponse.DeadLetter, ex.Message);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error message: {Message}", ex.Message);
                return new ProcessResponse(MessageProcessResponse.DeadLetter, ex.Message);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                consumer = new ServiceBusConsumer<SendEmailMessage>(ActionTypes.Queue);
                consumer.Register(ProcessMessageAsync);
                logger.Debug("Consumer {Consumer} registered", typeof(SendEmailWorker).Name);

                while (!stoppingToken.IsCancellationRequested)
                {
                    logger.Debug("Received message running at {Date}", DateTimeOffset.Now);
                    await Task.Delay(60000, stoppingToken);
                }
            }
            catch (TaskCanceledException taskEx)
            {
                logger.Error(taskEx, taskEx.Message);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
            finally
            {
                consumer?.Deregister();
            }
        }
    }
}