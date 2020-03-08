using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Users.Infrastructure;
using Users.Models;

namespace IdentityDemo.Controllers
{
    [Authorize]
    [RoutePrefix("Admin/Sigurnost")]
    public class SecurityController : Controller
    {        
        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Index()
        {
            string userId = User.Identity.GetUserId();

            var model = new SecurityViewModel
            {
                TwoFactorEnabled = await UserManager.GetTwoFactorEnabledAsync(userId),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthManager.TwoFactorBrowserRememberedAsync(userId)
            };

            return View(model);
        }

        [HttpPost]
        [Route("Omoguci2fa")] // Da je RESTful moglo bi Admin/Sigurnost
        [ValidateAntiForgeryToken]        
        public async Task<ActionResult> EnableTwoFactorAuth()
        {
            // Omoguci 2FA
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), enabled: true);
            
            bool isPersistant = ((System.Web.Security.FormsIdentity)User.Identity).Ticket.IsPersistent;
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistant, rememberBrowser: false);
            }
            // Ima neka greska
            return RedirectToAction("Sigurnost", "Admin");
        }

        [HttpPost]
        [Route("Onemoguci2fa")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuth()
        {
            // Omoguci 2FA
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), enabled: false);

            var isPersistant = ((System.Web.Security.FormsIdentity)User.Identity).Ticket.IsPersistent;
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistant, rememberBrowser: false);
            }
            // Ima neka greska
            return RedirectToAction("Sigurnost", "Admin");
        }


        // Alatke
        private AppUserManager UserManager
        {
            get => HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
        }
        private AppSignInManager SignInManager
        {
            get => HttpContext.GetOwinContext().Get<AppSignInManager>();
        }
        private IAuthenticationManager AuthManager
        {
            get => HttpContext.GetOwinContext().Authentication;
        }
    }
}