namespace LyricsApp.Application.Common.Models;


public record EmailMessage(IEnumerable<string> To, string Subject, string Content);


