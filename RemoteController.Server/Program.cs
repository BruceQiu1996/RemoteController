using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RemoteController.Server.HostService;
using Serilog;

namespace RemoteController.Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureServices((hostContext, services) =>
            {
                services.AddControllers();
                services.AddEndpointsApiExplorer();
                services.AddSingleton<ConnectionsManager>();
                services.AddHostedService<HostTcpService>();
            }).UseSerilog((context, logger) =>
            {
                logger.WriteTo.Console();
            });
            builder.Services.AddControllers();
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var app = builder.Build();
            app.Urls.Add("http://*:5000");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint($"/swagger/v1/swagger.json", "v1");
            });
            app.MapControllers();

            app.Run();
        }
    }
}
