
namespace MailKitExample
{
    using MimeKit;
    using MimeKit.Utils;
    using System.Threading.Tasks;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Joey", "joey@friends.com"));
            message.To.Add(new MailboxAddress("Alice", "alice@wonderland.com"));
            message.Subject = "How you doin?";

            var builder = new BodyBuilder();

            // Set the plain-text version of the message text
            builder.TextBody = @"Hey Alice,

What are you up to this weekend? Monica is throwing one of her parties on
Saturday and I was hoping you could make it.

Will you be my +1?

-- Joey
";

            // In order to reference selfie.jpg from the html text, we'll need to add it
            // to builder.LinkedResources and then use its Content-Id value in the img src.
            var image = builder.LinkedResources.Add(@"C:\Users\Joey\Documents\Selfies\selfie.jpg");
            image.ContentId = MimeUtils.GenerateMessageId();

            // Set the html version of the message text
            builder.HtmlBody = string.Format(@"<p>Hey Alice,<br>
<p>What are you up to this weekend? Monica is throwing one of her parties on
Saturday and I was hoping you could make it.<br>
<p>Will you be my +1?<br>
<p>-- Joey<br>
<center><img src=""cid:{0}""></center>", image.ContentId);

            // We may also want to attach a calendar event for Monica's party...
            builder.Attachments.Add(@"C:\Users\Joey\Documents\party.ics");

            // Now we just need to set the message body and we're done
            message.Body = builder.ToMessageBody();

        }

        /// <summary>
        /// Sends the given email message to the given addresses.
        /// </summary>
        /// <param name="emailMessage">The email message.</param>
        /// <param name="logoPath">The logo path.</param>
        /// <returns>A <see cref="Task"/> representing any asynchronous operation.</returns>
        private async Task SendEmail(EmailMessage emailMessage, string logoPath)
        {
            var message = new MimeMessage();

            if (emailMessage.FromAddresses.Count == 0)
            {
                emailMessage.FromAddresses.Add(new EmailAddress { Address = this.smtpConfiguration.FromAddress, Name = this.smtpConfiguration.FromName });
            }

            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.Cc.AddRange(emailMessage.CcAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.Bcc.AddRange(emailMessage.BccAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.Subject = emailMessage.Subject;


            if (emailMessage.IsHtml)
            {
                // Todo: Check this
                var builder = new BodyBuilder();

                // Todo: Add correct image somehow!!!
                //var image = builder.LinkedResources.Add(logoPath);
                var image = builder.LinkedResources.Add("C:\\Users\\Tim Hammer\\OneDrive - IVU Softwareentwicklung GmbH\\Git\\ivu-portal\\src\\IVU.Portal.Server\\bin\\publish\\IVU.Portal.Clients\\dist\\images\\default\\logo.png");
                image.ContentId = MimeUtils.GenerateMessageId();
                builder.HtmlBody = emailMessage.Body.Replace("{Logo}", image.ContentId);
                message.Body = builder.ToMessageBody();
            }
            else
            {
                message.Body = new TextPart("plain") { Text = emailMessage.Body };
            }

            using var emailClient = new SmtpClient();
            if (!this.smtpConfiguration.UseSsl)
            {
                emailClient.ServerCertificateValidationCallback = (sender2, certificate, chain, sslPolicyErrors) => true;
            }

            await emailClient.ConnectAsync(this.smtpConfiguration.ServerAddress, this.smtpConfiguration.Port, this.smtpConfiguration.UseSsl).ConfigureAwait(false);
            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

            if (!string.IsNullOrWhiteSpace(this.smtpConfiguration.UserName))
            {
                await emailClient.AuthenticateAsync(this.smtpConfiguration.UserName, this.smtpConfiguration.Password).ConfigureAwait(false);
            }

            await emailClient.SendAsync(message).ConfigureAwait(false);
            await emailClient.DisconnectAsync(true).ConfigureAwait(false);
        }
    }
}
