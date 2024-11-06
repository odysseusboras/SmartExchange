using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartExchange.Database;
using SmartExchange.Model.Configuration;
using SmartExchange.SignalR;
using SmartExchange.TradingProviders;

namespace SmartExchange
{
    internal class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
#if DEBUG
                   webBuilder.UseUrls("http://localhost:5001");
#else
                   webBuilder.UseUrls("http://localhost:5001");
#endif          

                   _ = webBuilder.Configure(app =>
                   {
                       ILogger<Program> logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();
                       logger.LogInformation("Starting SignalR server...");

                       _ = app.UseCors("AllowAllOrigins");

                       _ = app.UseRouting();

                       _ = app.UseEndpoints(endpoints =>
                       {
                           _ = endpoints.MapHub<TradingHub>("/tradingHub");

                           logger.LogInformation("SignalR Hub is running on /tradingHub");
                       });
                   });
               })
                .ConfigureAppConfiguration((context, config) =>
                {
                    _ = config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                              .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
#if DEBUG
                              .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true);
#else
                              .AddJsonFile("appsettings.release.json", optional: true, reloadOnChange: true);
#endif
                })
                .ConfigureServices((context, services) =>
                {

                    _ = services.AddSignalR();

                    services.AddScoped<TradingHubService>();

                    _ = services.AddCors(options =>
                    {
                        options.AddPolicy("AllowAllOrigins", builder =>
                        {
                            builder.AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .SetIsOriginAllowed(origin => true)
                                    .AllowCredentials();
                        });
                    });


                    IConfiguration configuration = context.Configuration;

                    _ = services.AddDbContext<ExchangeDBContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("AppDbContext")));

                    _ = services.Configure<AppSettings>(configuration.GetSection("Settings"));

                    _ = services.AddSingleton<ITradingProvider>(provider =>
                    {
                        AppSettings settings = provider.GetRequiredService<IOptions<AppSettings>>().Value;
                        return settings.TradingProvider?.Name switch
                        {
                            "Binance" => new BinanceProvider(settings),
                            _ => throw new InvalidOperationException("Unknown trading provider")
                        };
                    });

                    services.AddHostedService<ScheduledJobService>();

                });

        }

        private static async Task Main(string[] args)
        {
            IHostBuilder hostBuilder = CreateHostBuilder(args);


#if DEBUG
            IHost host = hostBuilder.Build();
#else
            IHost host = hostBuilder.UseWindowsService().Build();
#endif
            await host.RunAsync();
        }
    }
}
