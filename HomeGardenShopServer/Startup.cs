using System;
using System.Net;
using HomeGardenShopServer.Interface;
using HomeGardenShopServer.Models;
using HomeGardenShopServer.Moduls;
using HomeGardenShopServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ninject;

namespace HomeGardenShopServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
            }));
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
                options.HttpsPort = 443;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                Console.WriteLine("UseDeveloperExceptionPage");
            }
            
            app.UseRouting();
            app.UseGrpcWeb();
            app.UseCors();
            app.UseHttpsRedirection();
            app.UseEndpoints(endpoints =>
            {
               
                endpoints.MapGrpcService<GreeterService>().EnableGrpcWeb()
                                                  .RequireCors("AllowAll");

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                }); 
            });
            Test();
        }

        public async void Test()
        {
            IKernel kernel = new StandardKernel(new ProductNinjectModule());
            IService<Product> productService = kernel.Get<IService<Product>>();
            List<Product> products = new List<Product>();
            var list = await productService.GetAll();
            Console.WriteLine(list.Count());
        }
    }
}

