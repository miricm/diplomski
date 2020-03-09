using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Users.Infrastructure;
using Users.Models;

namespace IdentityDemo.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // currentUrl = Url sa kojeg je pozvana akcija
        [AllowAnonymous]
        public ViewResult Register(string returnUrl, string currentUrl)
        {
            ViewBag.ReturnUrl = returnUrl == null ? currentUrl : returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    DateRegistered = DateTime.Now,
                    ProfilePicture = UserManager.SetDefaultProfilePicture()
                };

                var createResult = await UserManager.CreateAsync(user, model.Password);

                if (createResult.Succeeded)
                {
                    // await SignInManager.SignInAsync(user, isPersistent: model.RememberMe, rememberBrowser: false);
                    await UserManager.AddToRoleAsync(user.Id, "Korisnik");
                    return RedirectToAction("SendVerificationEmail", new { userId = user.Id });
                }
                // Kreiranje naloga nije uspelo
                AddErrorsFromResult(createResult);
            }
            // Model ne valja, prikazi pogled ponovo
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> SendVerificationEmail(string userId)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userId);
            var callbackUrl = Url.Action(
                actionName: "ConfirmEmail",
                controllerName: "Account",
                routeValues: new { userId, code },
                protocol: Request.Url.Scheme
            );

            await UserManager.SendEmailAsync(
                userId,
                subject: "[WEBTECH] Potvrdite vaš e-mail",
                body: $"Kliknite na link kako biste verifikovali nalog:<br> <a href='{callbackUrl}'>Verifikuj nalog</a>"
            );

            // Samo radi testiranja
            // ViewBag.CallbackUrl = callbackUrl;
            return View("DisplayEmail");
        }

        [AllowAnonymous] // Metodu pokrece klik na link sa emaila, ili sa aplikacije ako je email iskljucen
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var confirmResult = await UserManager.ConfirmEmailAsync(userId, code);
            return View(confirmResult.Succeeded ? "ConfirmEmail" : "Error");
        }

        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            string userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }

            // Dohvatanje svih 2FA provajdera
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose })
                                           .ToList();

            return View(new SendCodeViewModel
            {
                Providers = factorOptions,
                ReturnUrl = returnUrl,
                RememberMe = rememberMe
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }
            // Generisi token i posalji
            if(!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { provider = model.SelectedProvider, returnUrl = model.ReturnUrl, rememberMe = model.RememberMe });
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            if(!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            
            return View(new VerifyCodeViewModel
            {
                Provider = provider,
                ReturnUrl = returnUrl,
                RememberMe = rememberMe
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginResult = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);

            switch (loginResult)
            {
                case SignInStatus.Success:
                    return Redirect(string.IsNullOrEmpty(model.ReturnUrl) ? "/" : model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Kod neispravan!");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string currentUrl)
        {
            ViewBag.ReturnUrl = returnUrl == null ? currentUrl : returnUrl.Replace("PostComment", "LoadPost");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel details, string returnUrl)
        {
            //ClaimsIdentity identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            //AuthManager.SignOut();
            //AuthManager.SignIn(new AuthenticationProperties { IsPersistent = true }, identity);

            if (!ModelState.IsValid)
            {
                return View(details);
            }

            // Korisnik mora imati verifikovan e-mail
            var user = await UserManager.FindByNameAsync(details.UserName);
            if (user != null)
            {
                if (!user.EmailConfirmed)
                {
                    return RedirectToAction("SendVerificationEmail", new { userId = user.Id });
                }
            }

            var loginResult = await SignInManager.PasswordSignInAsync(details.UserName, details.Password, details.RememberMe, false);

            switch (loginResult)
            {
                case SignInStatus.Success:
                    return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { returnUrl, rememberMe = details.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Informacije koje ste uneli su pogrešne!");
                    return View(details);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff(string currentUrl, string returnUrl)
        {
            AuthManager.SignOut();
            return Redirect(string.IsNullOrEmpty(currentUrl) ? "/" : currentUrl);
        }

        // Alatke
        private IAuthenticationManager AuthManager
        {
            get => HttpContext.GetOwinContext().Authentication;
        }
        private AppUserManager UserManager
        {
            get => HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
        }
        private AppSignInManager SignInManager
        {
            get => HttpContext.GetOwinContext().Get<AppSignInManager>();
        }
        private void AddErrorsFromResult(params IdentityResult[] results)
        {
            foreach (var result in results)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }
        }
    }
}