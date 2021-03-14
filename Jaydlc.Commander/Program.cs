using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jaydlc.Commander.Client;
using Jaydlc.Commander.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jaydlc.Commander
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                CreateHostBuilder(args).Build().Run();
            }
            else
            {
                new CommanderClient().Run(args[0]);
            }
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    webBuilder => { webBuilder.UseStartup<Startup>(); }
                );
    }
}