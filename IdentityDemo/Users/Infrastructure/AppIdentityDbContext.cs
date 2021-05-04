using Users.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Users.Infrastructure
{
    public class AppIdentityDbContext : IdentityDbContext<AppUser>
    {
        public AppIdentityDbContext() : base("IdentityDb") 
        {
            Database.SetInitializer(new IdentityDbInit());
            Database.Initialize(true);
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public static AppIdentityDbContext Create()
        {
            return new AppIdentityDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    public class IdentityDbInit : CreateDatabaseIfNotExists<AppIdentityDbContext>
    {
        protected override void Seed(AppIdentityDbContext context)
        {
            // add initial data
            base.Seed(context);
        }
    }
}