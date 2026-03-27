using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Polly;
using Polly.Extensions.Http;
using WIRM.API.Services;
using WIRM.API.Interface;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using WIRM.API.Models.Request;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "WIRM.API", Version  = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer Scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi();
builder.Services.AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("User.Read", policy =>
    {
        policy.RequireScope("User.Read");
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWirmUI", policy =>
    {
        policy.WithOrigins(configuration["CORS:Origin"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHttpClient<IOPRService, OPRService>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
builder.Services.AddHttpClient<ISEService, SEService>() 
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());


builder.Services.AddMemoryCache();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IOPRService, OPRService>();
builder.Services.AddScoped<ITicketEmailService, TicketEmailService>();
builder.Services.AddScoped<IArrayCustomerOnBoardingEmailService, ArrayCustomerEmailOnBoardingService>();
builder.Services.AddScoped<ISEService, SEService>();
builder.Services.AddSingleton<IWeightingCalculator, WeightingCalculator>();
builder.Services.AddSingleton<IAttachmentUploader, AttachmentUploader>();
builder.Services.AddScoped<IWorkItemCreatorService, WorkItemCreatorService>();
builder.Services.AddScoped<ICustomerOnboardingWorkItemService, CustomerOnboardingWorkItemService>();

#if DEBUG
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 650L * 1024 * 1024; // ~650 MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 650L * 1024 * 1024;
});
#else
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 650L * 1024 * 1024;
});
#endif

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowWirmUI");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Exponential backoff
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)); // Break after 5 failures for 30s
}
