using System.Threading.Tasks;
using Jaydlc.Commander.Client;
using Jaydlc.Commander.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Jaydlc.Commander
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                await CreateHostBuilder(args).Build().RunAsync();
            }
            else
            {
                await new CommanderClient().Run(args[0]);
            }
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();

                        webBuilder.UseUrls(
                            "https://0.0.0.0:8081", "http://0.0.0.0:8080"
                        );
                    }
                );
    }
}