using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.EntityFrameworkCore;
using DatabaseLayer.Models;

namespace DatabaseLayer
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(): base()
        {
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { 
           
        }
        private IConfiguration Configuration { get; }




        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BlogDB;Trusted_Connection=True;");

                optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

            }
        }

    }


    //public class AppDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<AppDbContext>
    //{
    //    public AppDbContext CreateDbContext(string[] args)
    //    {
    //        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    //        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BlogDB;Trusted_Connection=True;");

    //        return new AppDbContext(optionsBuilder.Options);
    //    }
    //}
}
