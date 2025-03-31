using Microsoft.AspNetCore.Identity.UI.Services;

namespace TodoRESTApi.Core.ExternalHelperInterface;

public interface ICustomEmailSender: IEmailSender
{
    
    /// <summary>
    /// Sends an email using a specified template.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="templatePath">The file path of the email template.</param>
    /// <param name="viewData">The View Data used to populate the template.</param>
    /// <returns>A task representing the asynchronous email sending operation.</returns>
    Task SendEmailWithTemplateAsync(string email, string subject, string templatePath, Dictionary<string, object> viewData);
}