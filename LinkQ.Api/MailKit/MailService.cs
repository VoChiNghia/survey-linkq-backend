using LinkQ.Core;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace LinkQ.Api.MailKit
{
    public class MailService : IMailService
    {
        #region Fields
        private readonly MailConfig _mailConfig;
        #endregion

        #region Ctor

        public MailService(MailConfig mailConfig)
        {
            _mailConfig = mailConfig;
        }

        #endregion


        #region Method

        public async Task SendAsync(MailRequest mailRequest)
        {
            var email = CreateEmailMessage(mailRequest);
            await SendAsync(email);
        }

        public void Send(MailRequest mailRequest)
        {
            var email = CreateEmailMessage(mailRequest);
            Send(email);
        }

        #endregion

        #region Utils

        private MimeMessage CreateEmailMessage(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailConfig.DisplayName, _mailConfig.Address));

            var toEmails = mailRequest?.ToEmail!.Split(";");
            var toMailBoxs = toEmails?.Select(add => MailboxAddress.Parse(CommonHelper.EnsureSubscriberEmailOrThrow(add)));
            email.To.AddRange(toMailBoxs);

            if (!string.IsNullOrEmpty(mailRequest?.Cc))
            {
                var ccEmails = mailRequest.Cc.Split(";");
                var ccMailBoxs = ccEmails.Select(add => MailboxAddress.Parse(CommonHelper.EnsureSubscriberEmailOrThrow(add)));
                email.Cc.AddRange(ccMailBoxs);
            }

            if (!string.IsNullOrEmpty(mailRequest?.Bcc))
            {
                var bccEmails = mailRequest.Bcc.Split(";");
                var bccMailBoxs = bccEmails.Select(add => MailboxAddress.Parse(CommonHelper.EnsureSubscriberEmailOrThrow(add)));
                email.Bcc.AddRange(bccMailBoxs);
            }

            email.Subject = mailRequest?.Subject;
            var builder = new BodyBuilder();
            if (mailRequest?.Attachments != null)
            {
                byte[] fileBytes;
                foreach (var file in mailRequest.Attachments)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }
            builder.HtmlBody = mailRequest?.Body;
            email.Body = builder.ToMessageBody();
            return email;
        }

        private void Send(MimeMessage mailMessage)
        {
            using var client = new SmtpClient();
            try
            {
                SecureSocketOptions socketOptions = _mailConfig.UseSSL ? SecureSocketOptions.SslOnConnect : (_mailConfig.UseTLS ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                client.Connect(_mailConfig.Host, _mailConfig.Port, socketOptions);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_mailConfig.Address, _mailConfig.Password);

                client.Send(mailMessage);

                //client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                //client.AuthenticationMechanisms.Remove("XOAUTH2");
                //client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
                //client.Send(mailMessage);
            }
            catch
            {
                //log an error message or throw an exception, or both.
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        private async Task SendAsync(MimeMessage mailMessage)
        {
            using var client = new SmtpClient();
            try
            {
                SecureSocketOptions socketOptions = _mailConfig.UseSSL ? SecureSocketOptions.SslOnConnect : (_mailConfig.UseTLS ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                await client.ConnectAsync(_mailConfig.Host, _mailConfig.Port, socketOptions);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_mailConfig.Address, _mailConfig.Password);

                await client.SendAsync(mailMessage);
            }
            catch
            {
                //log an error message or throw an exception, or both.
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }

        #endregion Utils
    }
}
