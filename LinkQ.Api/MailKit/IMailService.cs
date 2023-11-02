namespace LinkQ.Api.MailKit
{
    public interface IMailService
    {
        Task SendAsync(MailRequest mailRequest);
        void Send(MailRequest mailRequest);
    }
}
