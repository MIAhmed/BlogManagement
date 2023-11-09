using AuthenticationLayer;
using BlogService.Handlers.Commands;
using BlogService.Handlers.Queries;
using CachingLayer;
using DatabaseLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using APIGateway;
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


            services.AddMemoryCache();


            // getting secret key from the config that will be passed to the authentication layer for generating and validating JWT tokens 
            var authService = new AuthenticationLayer.AuthService(Configuration.GetValue<string>("AuthSecretKey"));
            services.AddSingleton(authService);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = authService.GetTokenValidationParameters();
           });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtAuthenticationDefaults.AuthenticationScheme;
            })
             .AddScheme<JwtAuthenticationOptions, JwtAuthenticationHandler>(JwtAuthenticationDefaults.AuthenticationScheme, options => { });

            services.AddAuthorization();



            services.AddAuthorization();
            // Registering authentication layer to be able to use with the controller
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        // Applying the token validation rules defined in authentication layer
            //        options.TokenValidationParameters = authService.GetTokenValidationParameters();
            //    });

            services.AddDbContext<AppDbContext>(options =>
            {
                string connectionString = "";
                if(_env.IsDevelopment())
                {
                    // getting connection string from config file for the localDB
                    connectionString = Configuration.GetConnectionString("SQLConnectionDev");
                }
                else
                {
                    // getting connection string from config file for the Docker env
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

            // To enable Rate limiting using API gateway
            app.UseRateLimiting();

            app.UseAuthentication();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseAuthentication();
            //app.UseAuthorization();
        }
    }
}



// docker-compose build

// docker-compose up