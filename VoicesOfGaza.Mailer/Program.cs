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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

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

#if DEBUG
app.UseCors("AllowSpecificOrigin");
#endif

app.MapPost("/mail", async (IResend resend, MailRequest request, HttpContext context) =>
{
    try
    {
        var variableDependencyValues = new string[4] { AppConfiguration.ResendKey, AppConfiguration.DefaultSubject, AppConfiguration.ToEmail, AppConfiguration.FromEmail };
        if (variableDependencyValues.Any(value => string.IsNullOrEmpty(value)))
        {
            throw new Exception("Environment configuration incomplete");
        }

        var message = new EmailMessage
        {
            From = $"Voices Of Gaza <{AppConfiguration.FromEmail}>",
            Subject = AppConfiguration.DefaultSubject,
            HtmlBody = request.Text
        };

        message.To.Add(AppConfiguration.ToEmail);
        if (request.Email != null)
        {
            var replyTo = !string.IsNullOrEmpty(request.Name)
            ? $"{request.Name} <{request.Email}>"
            : request.Email;
            message.ReplyTo = [replyTo];
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
    catch (Exception ex)
    {
        return Results.Json(data: ex.Message, statusCode: 500);
    }

    var origin = AppConfiguration.GetOrigin(context);
    context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
    return Results.Ok();
})
.WithName("PostMailRequest");

app.Run();
