using API.Application.Interfaces;
using API.Application.Services;
using API.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StorageLib.DB.Redis;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<Service>();
builder.Services.AddSingleton<RedisService>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5009);
});

var app = builder.Build();

app.MapControllers();

app.Run();