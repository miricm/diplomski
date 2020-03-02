namespace IdentityDemo.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Users.Infrastructure;
    using Users.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<AppIdentityDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "Users.Infrastructure.AppIdentityDbContext";
        }

        protected override void Seed(AppIdentityDbContext context)
        {
            //if (!System.Diagnostics.Debugger.IsAttached)
            //     System.Diagnostics.Debugger.Launch();

            //var userManager = new UserManager<AppUser>(new UserStore<AppUser>(context));
            //var roleManager = new RoleManager<AppRole>(new RoleStore<AppRole>(context));

            //userManager.AddToRoles(user.Id, "Korisnik");
            //userManager.AddToRoles(autor.Id, "Korisnik", "Autor");
            //userManager.AddToRoles(moderator.Id, "Korisnik", "Autor", "Moderator");
            //userManager.AddToRoles(admin.Id, "Korisnik", "Autor", "Moderator", "Administrator");

            //context.SaveChanges();
        }
    }
}