using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using ContractPulseAI.API.Repositories.Implementation;
using ContractPulseAI.API.Repositories.Interface;
using ContractPulseAI.API.Services.Implementation;
using ContractPulseAI.API.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ApplicationDbContext
builder.Services.AddDbContext<ContractPulseAI.API.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// Repositories
builder.Services.AddScoped<IPricingRepository, PricingRepository>();
builder.Services.AddScoped<IRfpRepository, RfpRepository>();

// Services
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IRfpService, RfpService>();
builder.Services.AddScoped<ISowService, SowService>();
builder.Services.AddScoped<ISowGenerationClient, SowGenerationClient>();

// ==========================================
// 3. Azure AI Foundry Client Registration (DI)
// ==========================================
//builder.Services.AddScoped<AgentsClient>(sp =>
//{
//    var config = sp.GetRequiredService<IConfiguration>();

//    string endpoint = config["AzureFoundrySettings:ProjectConnectionString"]
//        ?? throw new InvalidOperationException("ProjectConnectionString is missing from configurations.");

//    // Pull the single ApiKey string from your configuration file
//    string apiKey = config["AzureFoundrySettings:ApiKey"]
//        ?? throw new InvalidOperationException("ApiKey is missing from configurations.");

//    // Pass the single apiKey string to match your 1-argument constructor definition
//    var customCredential = new ContractPulseAI.API.Services.Implementation.CustomTokenCredentialProvider(apiKey);

//    // FIX: Pass the raw string endpoint variable directly instead of wrapping it in a new Uri()
//    return new AgentsClient(endpoint, customCredential);
//});

builder.Services.AddScoped<AgentsClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    string endpoint = config["AzureFoundrySettings:ProjectConnectionString"]
        ?? throw new InvalidOperationException("ProjectConnectionString is missing from configurations.");

    // Retrieve your Entra App Registration credentials cleanly
    string tenantId = config["AzureAd:TenantId"] ?? throw new Exception("TenantId missing from configurations.");
    string clientId = config["AzureAd:ClientId"] ?? throw new Exception("ClientId missing from configurations.");
    string clientSecret = config["AzureAd:ClientSecret"] ?? throw new Exception("ClientSecret missing from configurations.");

    // Pass the 3 identity parameters to match the updated constructor definition
    var customCredential = new ContractPulseAI.API.Services.Implementation.CustomTokenCredentialProvider(tenantId, clientId, clientSecret);

    return new AgentsClient(endpoint, customCredential);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Skip HTTPS redirect in dev: the frontend calls the plain http port, and redirecting
// breaks CORS (preflight/simple requests get a cross-origin redirect response).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendDev");

app.UseAuthorization();

app.MapControllers();

app.Run();
