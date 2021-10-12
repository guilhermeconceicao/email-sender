using Email.Sender.BlobStorage;
using Email.Sender.CrossCutting.Envs;
using Email.Sender.Domain.Services;
using Email.Sender.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;

namespace Email.Sender
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISendGridClient>(s => new SendGridClient(new SendGridClientOptions
            {
                ApiKey = EnvironmentVars.EmailApiKey
            }));

            services.AddSingleton<IBlobStorage, AzureBlobStorage>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddHostedService<SendEmailWorker>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }
}