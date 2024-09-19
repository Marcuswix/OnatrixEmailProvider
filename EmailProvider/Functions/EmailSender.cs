using System;
using Azure;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Azure.Communication.Email;

namespace EmailProvider.Functions
{
    public class EmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        [FunctionName("EmailSender")]
        public static async Task Run([ServiceBusTrigger("email_request", Connection = "ServiceBusSender")]string message, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");

            // Anta att meddelandet innehåller en e-postadress (eller annan nödvändig data)
            var emailTo = message;  // Exempel: meddelandet är en e-postadress
            await SendEmailAsync(emailTo);

            log.LogInformation($"Confirmation email sent to {emailTo}");
        }

        public static async Task SendEmailAsync(string recipientEmail)
        {
            var connectionString = Environment.GetEnvironmentVariable("CommunicationServices");
            var emailClient = new EmailClient(connectionString);

            var emailContent = new EmailContent("Bekräftelse för din förfrågan")
            {
                PlainText = "Tack för din förfrågan. Detta är ditt bekräftelsemail."
            };

            var sender = "DoNotReply@bced485c-da7b-4d5a-9378-53c65f1ec98a.azurecomm.net"; // Använd din verifierade avsändaradress
            var emailMessage = new EmailMessage(sender, recipientEmail, emailContent);

            try
            {
                var response = await emailClient.SendAsync(WaitUntil.Completed, emailMessage);
                Console.WriteLine($"Email sent, MessageId = {response.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }
    }
}
