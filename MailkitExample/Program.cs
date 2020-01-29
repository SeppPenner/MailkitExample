﻿
namespace MailKitExample
{
    using System.IO;
    using System.Reflection;

    using MimeKit;
    using MimeKit.Utils;
    using System.Threading.Tasks;

    using MailKit.Net.Smtp;

    public static class Program
    {
        public static async Task Main(string[] args)
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
            var logoPath = $"{Path.Combine(currentLocation, "Example.png")}";
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
}
