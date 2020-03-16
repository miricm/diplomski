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
            string userId = User.Identity.GetUserId();
            await UserManager.SetTwoFactorEnabledAsync(userId, enabled: true);
            var user = await UserManager.FindByIdAsync(userId);

            if (user != null)
            {
                return RedirectToAction("ConfirmPassword", new { returnUrl = "/Admin/Sigurnost" });
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
            string userId = User.Identity.GetUserId();
            await UserManager.SetTwoFactorEnabledAsync(userId, enabled: false);
            var user = await UserManager.FindByIdAsync(userId);

            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            // Ima neka greska
            return RedirectToAction("Sigurnost", "Admin");
        }

        [HttpGet]
        [Route("PotvrdiLozinku")]
        public ViewResult ConfirmPassword(string returnUrl)
        {
            return View(new ConfirmPasswordViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("PotvrdiLozinku")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmPassword(ConfirmPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userName = User.Identity.GetUserName();
            var loginResult = await SignInManager.PasswordSignInAsync(userName, model.Password, 
                isPersistent: false, shouldLockout: false);

            switch (loginResult)
            {
                case SignInStatus.Success:
                    return Redirect(model.ReturnUrl);
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", "Account", new { returnUrl = model.ReturnUrl, 
                                                                        rememberMe = false });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Uneta lozinka je neispravna!");
                    return View(model);
            }
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