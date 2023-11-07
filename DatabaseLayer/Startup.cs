using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }



        // This method gets called by the runtime. Use this method to add services 
        public void ConfigureServices(DbContextOptionsBuilder optionsBuilder)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            //var configuration = new ConfigurationBuilder()
            //    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            //    //.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            //    .AddJsonFile("appsettings.json")
            //    .Build();
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

            //services.AddSingleton<IConfiguration>(configuration);

                
            //services.AddDbContext<AppDbContext>(options =>
            //{   
            //    // getting connection string from config file
            //    //options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            //    var connectionString = configuration.GetConnectionString("DefaultConnection");
            //    options.UseSqlServer(connectionString);
            //});




            //builder.Services.AddDbContext<DataContext>(options =>
            //{
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            //});
            //builder.Services.AddDbContext<CommonProjectContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            //builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<CommonProjectContext>();
        }

        


        //From package manger console
        
        // To Add Migration
        // Add-Migration [MigraionName]


        // To Update Database
        //Update-Database


        // To revert to specific Migration 
        // Update-Database -Context MigrationName

    }
}
