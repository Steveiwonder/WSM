using Microsoft.OpenApi.Models;
using WSM.Server.Authentication;
using WSM.Server.BackgroundServices;
using WSM.Server.Configuration;
using WSM.Server.Services;
using WSM.Server.Services.Notifications;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "ApiKey",
        Scheme = "ApiKey",
        In = ParameterLocation.Header,
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});
var notificationTypes = builder.Configuration.GetSection("NotificationTypes").Get<string[]>();

if (notificationTypes != null)
{
    foreach (var notificationType in notificationTypes)
    {
        if ("twilio".Equals(notificationType, StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddSingleton<INotificationService, TwilioNotificationService>();
        }
        else if ("email".Equals(notificationType, StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddSingleton<INotificationService, EmailNotificationService>();
        }
    }
}
builder.Services.AddSingleton<INotificationSender, AggregateNotificationSender>();
builder.Services.AddHostedService<WSMHealthCheckBackgroundService>();
builder.Services.AddSingleton<WSMHealthCheckService>();
builder.Services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, options =>
    {

    });
builder.Services.AddHttpContextAccessor();


/*Configuration*/

var servers = builder.Configuration.GetSection("Servers").Get<IEnumerable<ServerConfiguration>>() ?? Array.Empty<ServerConfiguration>();

builder.Services.AddSingleton(servers);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Map("/ping", () => Results.StatusCode(200));
app.MapControllers();

app.Run();
