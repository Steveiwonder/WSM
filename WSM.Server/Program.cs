using Microsoft.OpenApi.Models;
using WSM.Server.Authentication;
using WSM.Server.BackgroundServices;
using WSM.Server.Configuration;
using WSM.Server.Services;

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
var notificationType = builder.Configuration.GetSection("NotificationType").Get<string>();

if ("whatsapp".Equals(notificationType, StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<INotificationService, WhatsAppNotificationService>();
}
else if ("email".Equals(notificationType, StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<INotificationService, EmailNotificationService>();
}
else
{
    builder.Services.AddSingleton<INotificationService, NullNotificationService>();
}
builder.Services.AddHostedService<WSMHealthCheckBackgroundService>();
builder.Services.AddSingleton<WSMHealthCheckService>();
builder.Services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, options =>
    {

    });
builder.Services.AddHttpContextAccessor();


/*Configuration*/


builder.Services.AddSingleton(builder.Configuration.GetSection("Servers").Get<IEnumerable<ServerConfiguration>>());
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

app.MapControllers();

app.Run();
