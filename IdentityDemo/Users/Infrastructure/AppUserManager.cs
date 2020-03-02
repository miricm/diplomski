using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Users.Models;
using Microsoft.Owin;
using System.IO;

namespace Users.Infrastructure
{
    public class AppUserManager : UserManager<AppUser>
    {
        public AppUserManager(IUserStore<AppUser> store) : base(store)
        {

        }
        public static AppUserManager Create(IdentityFactoryOptions<AppUserManager> options, IOwinContext context)
        {
            AppIdentityDbContext db = context.Get<AppIdentityDbContext>();
            AppUserManager manager = new AppUserManager(new UserStore<AppUser>(db));

            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireLowercase = true,
                RequireUppercase = false,
                RequireNonLetterOrDigit = false,
                RequireDigit = false
            };

            return manager;
        }

        // Dodatno
        public byte[] SetDefaultProfilePicture()
        {
            string imagePath = HttpContext.Current.Server.MapPath("~/Content/img/default_profile_picture.png");
            byte[] image = File.ReadAllBytes(imagePath);
            return image;
        }
    }
}