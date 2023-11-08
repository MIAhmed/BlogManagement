using BlogService.Handlers.Commands;
using BlogService.Handlers.Queries;
using CachingLayer;
using DatabaseLayer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BlogService
{
    public class Startup
    {
        private IWebHostEnvironment _env;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();


            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = "";
                //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                if(_env.IsDevelopment())
                {
                    // getting connection string from config file
                    connectionString = Configuration.GetConnectionString("SQLConnectionDev");
                }
                else
                {
                    connectionString = Configuration.GetConnectionString("SQLConnectionDocker");
                }
                options.UseSqlServer(connectionString);
                
            });

            // Register command processor for CQRS pattern
            services.AddTransient<BlogCommandHandler>();

            // Register query processor for CQRS pattern
            services.AddTransient<BlogQueryHandler>();

            //Register Redis for 
            services.AddSingleton<RedisCacheService>(provider => 
                new RedisCacheService(Configuration.GetConnectionString("RedisConnection")));

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetValue<string>("ConnectionStrings: RedisConnection");
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlogService", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        public void Configure(IApplicationBuilder app, AppDbContext dbContext)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlogService v1"));
            }
            else
            {
                // Enable automatic migration on deployment
                dbContext.Database.Migrate();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}



// docker-compose build

// docker-compose up