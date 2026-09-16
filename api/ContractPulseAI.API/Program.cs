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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendDev");

app.UseAuthorization();

app.MapControllers();

app.Run();
