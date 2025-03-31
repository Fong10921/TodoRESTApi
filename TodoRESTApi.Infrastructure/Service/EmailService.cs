using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.MailKitSmtp;
using Hangfire;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using TodoRESTApi.Core.ExternalHelperInterface;

namespace TodoRESTApi.Infrastructure.Service;
public class EmailService: ICustomEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config["SmtpSettings:SenderName"], _config["SmtpSettings:SenderEmail"]));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = htmlMessage };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config["SmtpSettings:Server"], int.Parse(_config["SmtpSettings:Port"]!),
            MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }

    // Implements default ASP.NET Identity email sender method
    /*public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await SendEmailWithTemplateAsync(email, subject, null, new { HtmlContent = htmlMessage });
    }*/
    
    [AutomaticRetry(Attempts = 5, DelaysInSeconds = [60, 300, 600])] // Retries after 1min, 5min, and 10min
    public async Task SendEmailWithTemplateAsync(string email, string subject, string templatePath, Dictionary<string, object> viewData)
    {
        // Configure FluentEmail to use Razor for templating
        Email.DefaultRenderer = new RazorRenderer();
        
        // Configure FluentEmail to use MailKit as SMTP sender
        var sender = new MailKitSender(new SmtpClientOptions
        {
            Server = _config["SmtpSettings:Server"],
            Port = int.Parse(_config["SmtpSettings:Port"]!),
            UseSsl = false,
            RequiresAuthentication = !string.IsNullOrEmpty(_config["SmtpSettings:Username"]),
            User = _config["SmtpSettings:Username"],
            Password = _config["SmtpSettings:Password"]
        });

        Email.DefaultSender = sender;
        
        // Load the template and populate with model data
        var emailMessage = await Email
            .From(_config["SmtpSettings:SenderEmail"], _config["SmtpSettings:SenderName"])
            .To(email)
            .Subject(subject)
            .UsingTemplateFromFile(templatePath, viewData)
            .SendAsync();

        if (emailMessage.Successful)
        {
            _logger.LogInformation($"Email sent successfully to {email} with subject '{subject}' at {DateTime.UtcNow}.");
        }
        else
        {
            _logger.LogError($"Failed to send email to {email}. Errors: {string.Join(", ", emailMessage.ErrorMessages)}");
        }
    }
    

}
    
