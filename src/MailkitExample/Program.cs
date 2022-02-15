// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace MailKitExample;

/// <summary>
/// The main program.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main method.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
    public static async Task Main()
    {
        const string UserName = "GMX-Email";
        const string Password = "Password";
        const string ReceiverAddress = "Receiver-GMX-Email";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Joey", UserName));
        message.To.Add(new MailboxAddress("Alice", ReceiverAddress));
        message.Subject = "How you doing?";

        var builder = new BodyBuilder();
        var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var logoPath = $"{Path.Combine(currentLocation ?? string.Empty, "Example.png")}";
        var image = builder.LinkedResources.Add(logoPath);
        image.ContentId = MimeUtils.GenerateMessageId();
        builder.HtmlBody = $@"<p>Hey Alice, ... <center><img src=""cid:{image.ContentId}""></center>";
        message.Body = builder.ToMessageBody();

        using var emailClient = new SmtpClient();

        await emailClient.ConnectAsync("mail.gmx.net", 465, true).ConfigureAwait(false);
        emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

        await emailClient.AuthenticateAsync(UserName, Password).ConfigureAwait(false);
        await emailClient.SendAsync(message).ConfigureAwait(false);
        await emailClient.DisconnectAsync(true).ConfigureAwait(false);
    }
}
