using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.CommonModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Sender.Services;
using Sender.Services.Interfaces;

namespace Sender
{
    public class Startup
    {
        private readonly IConfiguration Configuration;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RabbitMQConnection>(Configuration.GetSection(nameof(RabbitMQConnection)));
            services.AddSingleton<RabbitMQConnection>(provider => provider.GetRequiredService<IOptions<RabbitMQConnection>>().Value);
            
            services.AddSwaggerGen(opt => 
                {
                    opt.EnableAnnotations();
                    opt.SwaggerDoc("v1", new OpenApiInfo()
                    {
                        Title = "RabbitExample",
                        Description = "OneSenderOneReciever",
                        Version = "v1"
                    });
                });
            services.AddScoped<IRabbitMQExamplesService, RabbitMQExamplesService>();
            services.AddMvc(o => o.EnableEndpointRouting = false);
        }   

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger(o =>
            {
                o.RouteTemplate = "api/swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(uio => 
            {
                uio.SwaggerEndpoint($"/api/swagger/v1/swagger.json", "Sample API");
                uio.RoutePrefix = "api/swagger";
            });
            app.UseMvc();
        }
    }
}
