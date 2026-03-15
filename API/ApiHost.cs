namespace API;
using API.Application.Interfaces;
using API.Application.Services;
using API.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StorageLib.DB.Redis;
using System.Net;
public class ApiHost
{
    private CancellationTokenSource _cts;
    private Task _webHostTask;

    public void Start()
    {
        _cts = new CancellationTokenSource();

        _webHostTask = Task.Run(async () =>
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddControllers()
                .AddApplicationPart(typeof(API.Controllers.MainController).Assembly);

            builder.Services.AddScoped<IRepository, Repository>();
            builder.Services.AddScoped<Service>();
            builder.Services.AddSingleton<RedisService>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Any,5009);
            });

            var app = builder.Build();

            app.MapControllers();

            await app.RunAsync(_cts.Token);
        });
    }

    public async Task StopAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            try
            {
                await _webHostTask;
            }
            catch (OperationCanceledException)
            { }
        }
    }
}
