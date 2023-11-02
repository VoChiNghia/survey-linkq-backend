using Microsoft.AspNetCore.Http;

namespace LinkQ.Api.MailKit
{
    public class MailRequest
    {
        public string? ToEmail { get; set; }
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }
}
