namespace VoicesOfGaza.Mailer;

public static class AppConfiguration
{
    private static IConfiguration? _configuration;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static string DefaultSubject
    {
        get
        {
            return Environment.GetEnvironmentVariable("RESEND_DEFAULT_SUBJECT")
                ?? _configuration?["Resend:DefaultSubject"]
                ?? string.Empty;
        }
    }

    public static string FromEmail
    {
        get
        {
            return Environment.GetEnvironmentVariable("RESEND_FROM_EMAIL")
                ?? _configuration?["Resend:FromEmail"]
                ?? string.Empty;
        }
    }

    public static string ResendKey
    {
        get
        {
            return Environment.GetEnvironmentVariable("RESEND_KEY")
                ?? _configuration?["Resend:ApiKey"]
                ?? string.Empty;
        }
    }

    public static string ToEmail
    {
        get
        {
            return Environment.GetEnvironmentVariable("RESEND_TO_EMAIL")
                ?? _configuration?["Resend:ToEmail"]
                ?? string.Empty;
        }
    }
}
