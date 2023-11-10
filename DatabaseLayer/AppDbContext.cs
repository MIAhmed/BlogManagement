using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.EntityFrameworkCore;
using DatabaseLayer.Models;
using Microsoft.Extensions.Configuration;

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



        //From package manger console

        // To Add Migration
        // Add-Migration [MigraionName]


        // To Update Database
        //Update-Database


        // To revert to specific Migration 
        // Update-Database -Context MigrationName


    }



}
