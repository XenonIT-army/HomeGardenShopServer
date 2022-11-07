//using HomeGardenShopServer.Services;

//var builder = WebApplication.CreateBuilder(args);

//// Additional configuration is required to successfully run gRPC on macOS.
//// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

//// Add services to the container.
//builder.Services.AddGrpc();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//app.MapGrpcService<GreeterService>();
//app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

//app.Run();
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HomeGardenShopServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                   
                    webBuilder.UseKestrel((context, serverOptions) =>
                    {
                        serverOptions.Configure(context.Configuration.GetSection("Kestrel"))
                            .Endpoint("HTTPS", listenOptions =>
                            {
                                listenOptions.HttpsOptions.SslProtocols = SslProtocols.Tls12;
                                //listenOptions.HttpsOptions.ServerCertificate = new X509Certificate2("C:\\Users\\Administrator\\Desktop\\HomeGardenShopServer\\HomeGardenShopServer\\HomeGardenShopServer\\Properties\\cert.pfx", "2904");

                            });
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}

