namespace VoicesOfGaza.Mailer;

public static class AppConfiguration
{
    private static IConfiguration? _configuration;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static string[] AllowedHosts
    {
        get
        {
            string[] allowedHosts = (Environment.GetEnvironmentVariable("ALLOWED_HOSTS")
                ?? _configuration?["AllowedHosts"]
                ?? string.Empty).Split(';')
                ?? [];

            string[] allowedPorts = (Environment.GetEnvironmentVariable("ALLOWED_PORTS")
                ?? _configuration?["AllowedPorts"]
                ?? string.Empty).Split(';')
                ?? [];

            return [.. allowedHosts.Where(host => host != "*").Aggregate([], (List<string> hosts, string host) =>
            {
                var protocol = host.Contains("localhost") ? "http" : "https";
                hosts.Add($"{protocol}://{host}");

                foreach (var port in allowedPorts) {
                    hosts.Add($"{protocol}://{host}:{port}");
                }

                return hosts;
            })];
        }
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

    public static string GetOrigin(HttpContext context)
    {
        var origin = AllowedHosts.FirstOrDefault(host => host == context.Request.Headers.Origin);
        return origin ?? string.Empty;
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
