using Azure.AI.Projects;
using Azure.Identity;
using ContractPulseAI.API.Repositories.Implementation;
using ContractPulseAI.API.Repositories.Interface;
using ContractPulseAI.API.Services.Implementation;
using ContractPulseAI.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var endpoint = config["AzureFoundrySettings:ProjectEndpoint"]
        ?? throw new InvalidOperationException("ProjectEndpoint missing.");

    var tenantId = config["AzureAd:TenantId"]
        ?? throw new InvalidOperationException("TenantId missing.");

    var clientId = config["AzureAd:ClientId"]
        ?? throw new InvalidOperationException("ClientId missing.");

    var clientSecret = config["AzureAd:ClientSecret"]
        ?? throw new InvalidOperationException("ClientSecret missing.");

    var credential = new ClientSecretCredential(
        tenantId,
        clientId,
        clientSecret);

    return new AIProjectClient(
        new Uri(endpoint),
        credential);
});

builder.Services.AddScoped(sp =>
{
    var projectClient = sp.GetRequiredService<AIProjectClient>();
    return projectClient.GetProjectOpenAIClient();
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
