using System.ComponentModel;

namespace VoicesOfGaza.Mailer;

internal record MailRequest
{
    public string? Email { get; set; }

    [DefaultValue("John Smith")]
    public string? Name { get; set; }

    [DefaultValue("Hello world!")]
    public required string Text { get; set; }
}
