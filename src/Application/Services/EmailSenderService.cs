using LyricsApp.Application.Common.Configurations;
using LyricsApp.Application.Common.Models;
using LyricsApp.Application.Domain.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LyricsApp.Application.Services;

public class EmailSenderService: IEmailSender {

    private readonly EmailConfiguration _emailConfiguration;

    public EmailSenderService(EmailConfiguration emailConfiguration)
    {
        this._emailConfiguration = emailConfiguration;
    }

    public void SendEmail(EmailMessage emailMessage)
    {
        var email = CreateEmailMessage(emailMessage);
        Send(email);
    }


    private MimeMessage CreateEmailMessage( EmailMessage emailMessage)
    {
        var mail = new MimeMessage();
        mail.From.Add(new MailboxAddress(_emailConfiguration.UserName, _emailConfiguration.From));
        mail.To.AddRange(emailMessage.To.Select(x => new MailboxAddress(x, x)).ToList());
        mail.Subject = emailMessage.Subject;
        mail.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = emailMessage.Content };
        return mail;
    }

    private void Send(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            client.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.Port, SecureSocketOptions.StartTls);
            client.Authenticate(_emailConfiguration.From, _emailConfiguration.Password);
            client.Send(mailMessage);
        }
        catch(Exception ex)
        {
            //log an error message or throw an exception or both.
            throw new Exception(ex.Message);
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
    }
}