using System;
using Azure;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Communication.Email;
using EmailProvider.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace EmailProvider.Functions
{
    public class EmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly EmailClient _emailClient;

        public EmailSender(ILogger<EmailSender> logger, EmailClient emailClient)
        {
            _logger = logger;
            _emailClient = emailClient;
        }

        [FunctionName("EmailSender")]
        public async Task Run([ServiceBusTrigger("email_request", Connection = "ServiceBusSender")]string message, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");

            CallBackModel callback;

            try
            {
                callback = JsonConvert.DeserializeObject<CallBackModel>(message);

                if(callback != null || string.IsNullOrWhiteSpace(callback.Email))
                {
                    var response = await SendEmailAsync(callback.Email);

                    if(response is OkResult)
                    {
						log.LogInformation($"Confirmation email sent to {callback.Email}");
					}
                    else
                    {
						log.LogInformation($"Confirmation was NOT sent to {callback.Email}");
					}

                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, "Faild to deserialize Json Object");
                return;
            }
        }
        public async Task<IActionResult> SendEmailAsync(string recipientEmail)
        {

            var emailContent = new EmailContent("Onatrix Confirmation Email")
            {
                Html = "<html>\r\n<body style=\"font-family: Arial, sans-serif; margin: 0; padding: 0;\">\r\n<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n<tr>\r\n<td align=\"center\" style=\"background-color: #f8f9fa; padding: 20px;\">\r\n <table width=\"600\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background-color: #ffffff; padding: 20px; border-radius: 6px;\">\r\n <tr>\r\n                        <td align=\"center\" style=\"padding-bottom: 20px;\">\r\n                            <h1 style=\"font-size: 24px; color: #535656;\">We Have Recived Your Callback Request!</h1>\r\n                            <p style=\"font-size: 16px; color: #535656;\">Tack för din förfrågan. Vi har mottagit den och kommer att återkoppla snart.</p>\r\n <a href=\"https://example.com\" style=\"display: inline-block; padding: 10px 20px; background-color: #4F5955; color: #F2EDDC; text-decoration: none; border-radius: 4px; font-size: 16px;\">To Our website</a>\r\n                        </td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td style=\"padding: 10px 0; border-top: 1px solid #dee2e6; text-align: center; font-size: 12px; color: #adb5bd;\">\r\n                            Detta är ett automatiskt genererat meddelande, vänligen svara inte.\r\n                        </td>\r\n                    </tr>\r\n                </table>\r\n            </td>\r\n        </tr>\r\n    </table>\r\n</body>\r\n</html>\r\n",
                PlainText = "Tack för din förfrågan. Detta är ditt bekräftelsemail."
            };

            var sender = Environment.GetEnvironmentVariable("SenderDomain");

            var emailMessage = new EmailMessage(sender, recipientEmail, emailContent);

            try
            {
                var response = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);

                if(response.HasCompleted)
                {
                    Console.WriteLine($"Email sent, MessageId = {response.Id}");
                    return new OkResult();
                }  
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }

			return new BadRequestResult();
		}
    }
}
