
using LinkQ.Core;

namespace LinkQ.Api.MailKit
{
    public partial class MailConfig
    {
        public string? Address { get; set; }
        public string? DisplayName { get; set; }
        public string? Password { get; set; }
        public string? Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool UseSSL { get; set; } = true;
        public bool UseTLS { get; set; } = true;
    }
}
