using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RemoteController.Server.HostService;
using Serilog;
using System.Text;

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
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "lijun v1",
                        Description = "lijun v1版本接口"
                    });

                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        BearerFormat = "JWT",
                        Scheme = "Bearer"
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                        {
                            new OpenApiSecurityScheme{Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme,Id = "Bearer"}},new string[] { }
                        }
                        });
                });
                //添加认证
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(hostContext.Configuration["Jwt:JwtSecret"]!)),
                        ValidateIssuer = true,
                        ValidIssuer = hostContext.Configuration["System:Name"],
                        ValidateAudience = true,
                        ValidAudience = hostContext.Configuration.GetSection("Jwt:Audience").Get<string>(),
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(int.Parse(hostContext.Configuration["Jwt:ClockSkew"]!)), //过期时间容错值，解决服务器端时间不同步问题（秒）
                        RequireExpirationTime = true
                    };
                });
            }).UseSerilog((context, logger) =>
            {
                logger.WriteTo.Console();
            });
            builder.Services.AddControllers();
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var app = builder.Build();
            app.Urls.Add("http://*:5590");
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
