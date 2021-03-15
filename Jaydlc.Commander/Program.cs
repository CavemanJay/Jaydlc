using System;
using System.Threading.Tasks;
using Jaydlc.Commander.Client;
using Jaydlc.Commander.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Jaydlc.Commander
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length == 0)
            {
                await CreateHostBuilder(args).Build().RunAsync();
                return 0;
            }
            else
            {
                // Arguments should be host, port, tls
                return await new CommanderClient().Run(
                    args[0], int.Parse(args[1]), bool.Parse(args[2]), args[3]
                );
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