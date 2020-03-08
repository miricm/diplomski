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
using Microsoft.Owin.Security.DataProtection;

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
                RequireDigit = true
            };

            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<AppUser> 
            { 
                // Registrovanje provajdera, napisati u dokumentaciji
                Subject     = "Vaš sigurnosni kod", 
                BodyFormat  = "Vaš sigurnosni kod je {0}"
            });

            manager.EmailService = new EmailService();

            var dataProtectionProvider = options.DataProtectionProvider;
            if(dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<AppUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

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