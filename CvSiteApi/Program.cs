using CvSiteApi.Services;
using CvSiteApi.Services.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

// Configure GitHub Options from User Secrets / appsettings
builder.Services.Configure<GitHubOptions>(
    builder.Configuration.GetSection("GitHub"));

// Register GitHubService
builder.Services.AddSingleton<GitHubService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
