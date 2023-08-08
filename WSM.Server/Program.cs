using Microsoft.Extensions.DependencyInjection;
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
    options.AddSecurityDefinition("ApplicationId", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        BearerFormat = "ApplicationId",
        Scheme = "ApplicationId",
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
                    Id="ApplicationId"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddSingleton<INotificationService, WhatsAppNotificationServicerService>();
builder.Services.AddHostedService<WSMHealthCheckBackgroundService>();
builder.Services.AddSingleton<WSMHealthCheckService>();
builder.Services.AddAuthentication(ApplicationIdAuthenticationOptions.DefaultScheme)
    .AddScheme<ApplicationIdAuthenticationOptions, ApplicationIdAuthenticationHandler>(ApplicationIdAuthenticationOptions.DefaultScheme, options =>
    {

    });

/*Configuration*/


builder.Services.AddSingleton(new ApplicationIds() { Values = builder.Configuration.GetSection("ApplicationIds").Get<string[]>() });
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
