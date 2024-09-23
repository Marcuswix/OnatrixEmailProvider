using Azure.Communication.Email;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(EmailProvider.Functions.Startup))]

namespace EmailProvider.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Registrera EmailClient som en Singleton i DI
            builder.Services.AddSingleton(s =>
            {
                var connectionString = Environment.GetEnvironmentVariable("CommunicationServices");
                return new EmailClient(connectionString);
            });
        }
    }
}

