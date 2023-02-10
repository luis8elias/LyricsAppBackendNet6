using LyricsApp.Application.Common.Models;

namespace LyricsApp.Application.Domain.Interfaces;

public interface IEmailSender {
    void SendEmail(EmailMessage emailMessage);
}