using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace Jaydlc.Web
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Two-Stage logger initialization
            // Will catch any startup issues, allowing us to create more complex logger later with
            // DI services
            // https://github.com/serilog/serilog-aspnetcore#two-stage-initialization
            Log.Logger = new LoggerConfiguration().Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(ConfigureLogger)
                .ConfigureWebHostDefaults(
                    webBuilder => { webBuilder.UseStartup<Startup>(); }
                );

        private static void ConfigureLogger(HostBuilderContext context,
            IServiceProvider services, LoggerConfiguration configuration)
        {
            configuration.MinimumLevel
                .Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel
                .Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console();

            var env = services.GetRequiredService<IWebHostEnvironment>();
            if (env.IsProduction())
            {
                configuration.WriteTo.File(
                    new CompactJsonFormatter(), "/var/log/jaydlc/site.log",
                    LogEventLevel.Debug, rollingInterval: RollingInterval.Day
                );
            }
        }
    }
}