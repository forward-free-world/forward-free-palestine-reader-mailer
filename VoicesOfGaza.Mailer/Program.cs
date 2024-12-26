using System.ComponentModel;
using System.Diagnostics;
using Resend;
using VoicesOfGaza.Mailer;

var builder = WebApplication.CreateBuilder(args);

// Initialize AppConfiguration
AppConfiguration.Initialize(builder.Configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = AppConfiguration.ResendKey!;
});
builder.Services.AddSingleton<IResend, ResendClient>();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.MapPost("/mail", async (IResend resend, MailRequest request) =>
{
    try
    {
        var variableDependencyValues = new string[4] { AppConfiguration.ResendKey, AppConfiguration.DefaultSubject, AppConfiguration.ToEmail, AppConfiguration.FromEmail };
        if(variableDependencyValues.Any(value => string.IsNullOrEmpty(value)))
        {
            throw new Exception("Environment configuration incompleted");
        }

        var message = new EmailMessage
        {
            From = AppConfiguration.FromEmail,
            Subject = request.Subject ?? AppConfiguration.DefaultSubject,
            HtmlBody = request.Text
        };

        message.To.Add(AppConfiguration.ToEmail);
        if (request.Email != null)
        {
            message.ReplyTo = [request.Email];
        }

        if (resend == null)
        {
            throw new Exception("Resend not injected");
        }
        else
        {
            await resend.EmailSendAsync(message);
        }
    }
    catch(Exception ex)
    {
        return Results.Json(data: ex.Message, statusCode: 500);
    }

    return Results.Ok();
})
.WithName("PostMailRequest");

app.Run();

internal record MailRequest
{
    public string? Email { get; set; }

    [DefaultValue("Test Email")]
    public string? Subject { get; set; }

    [DefaultValue("Hello world!")]
    public required string Text { get; set; }
}