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
            if (!ModelState.IsValid)
            {
                return View();
            }
            // Generisi token i posalji
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { provider = model.SelectedProvider, returnUrl = model.ReturnUrl, rememberMe = model.RememberMe });
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            if (!await SignInManager.HasBeenVerifiedAsync())
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

            var loginResult = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code,
                                                           model.RememberMe, model.RememberBrowser);

            switch (loginResult)
            {
                case SignInStatus.Success:
                    return Redirect(string.IsNullOrEmpty(model.ReturnUrl) ? "/" : model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Kod koji ste uneli nije tačan!");
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

            // Korisnik mora imati verifikovan e-mail, ako nema salje mu se potvrda
            // Odkomentarisati kod ispod

            //var user = await UserManager.FindByNameAsync(details.UserName);
            //if (user != null)
            //{
            //    if (!await UserManager.IsEmailConfirmedAsync(user.Id))
            //    {
            //        return RedirectToAction("SendVerificationEmail", new { userId = user.Id });
            //    }
            //}

            var loginResult = await SignInManager.PasswordSignInAsync(details.UserName, details.Password, details.RememberMe, shouldLockout: false);
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

        [AllowAnonymous]
        public ViewResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null || !await UserManager.IsEmailConfirmedAsync(user.Id))
            {
                // Ne otkrivati da korisnik ne postoji ili da nije verifikovan email
                return View("ForgotPasswordConfirmation");
            }
            // Generisi kod i link
            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            var callbackUrl = Url.Action(
                actionName: "ResetPassword",
                controllerName: "Account",
                new { userId = user.Id, code },
                protocol: Request.Url.Scheme
            );
            // Posalji email sa kodom
            await UserManager.SendEmailAsync(user.Id, "[WEBTECH] Resetovanje lozinke",
                $"Resetujte lozinku klikom na link: <a href='{callbackUrl}'>link</a>.");

            return RedirectToAction("ForgotPasswordConfirmation", "Account");
        }

        // Pokrece se preko linka poslatog preko mail-a
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ViewResult ResetPassword(string code)
        {
            return string.IsNullOrEmpty(code) ? View("Error") : View(new ResetPasswordViewModel { Code = code });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Korisnik ne postoji, ne otkrivaj
                return View("ResetPasswordConfirmation");
            }
            // Resetuj lozinku
            var resetResult = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (resetResult.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrorsFromResult(resetResult);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(ExternalLoginListViewModel model, string provider)
        {
            var url = Url.Action("ExternalLoginCallback", "Account", new { returnUrl = model.ReturnUrl });
            // Zahteva preusmeravanja ka ekternom provajderu
            return new ChallengeResult(provider, url);
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return Redirect("/Account/Login");
            }

            var loginResult = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (loginResult)
            {
                case SignInStatus.Success:
                    return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { returnUrl, rememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Ponuditi korisnika da napravi nalog, ako nema
                    // ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirm", new ExternalLoginConfirmModel { Email = loginInfo.Email, ReturnUrl = returnUrl });
            }
        }

        // POST: /Account/ExternalLoginConfirm
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirm(ExternalLoginConfirmModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            if (ModelState.IsValid)
            {
                // Dohvati info o korisniku od provajdera
                var loginInfo = await AuthManager.GetExternalLoginInfoAsync();
                if (loginInfo == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = await UserManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    // Korisnik nema nalog, registruj ga
                    user = new AppUser 
                    {
                        UserName = loginInfo.DefaultUserName, 
                        Email = loginInfo.Email, 
                        DateRegistered = DateTime.Now,
                        ProfilePicture = UserManager.SetDefaultProfilePicture()
                    };

                    var createResult = await UserManager.CreateAsync(user);
                    var addToRole = await UserManager.AddToRoleAsync(user.Id, "Korisnik");
                    if (!createResult.Succeeded || !addToRole.Succeeded)
                    {
                        AddErrorsFromResult(createResult, addToRole);
                        return View(model);
                    }
                }
                // Korisnik vec ima nalog
                var addLoginResult = await UserManager.AddLoginAsync(user.Id, loginInfo.Login);
                if (addLoginResult.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return Redirect(string.IsNullOrEmpty(model.ReturnUrl) ? "/" : model.ReturnUrl);
                }
                AddErrorsFromResult(addLoginResult);
            }
            // Nevalidan model
            return View(model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ViewResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ViewResult ResetPasswordConfirmation()
        {
            return View();
        }
        
        public ViewResult AddPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPassword(AddPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string userId = User.Identity.GetUserId();
            if(!await UserManager.HasPasswordAsync(userId))
            {
                var addResult = await UserManager.AddPasswordAsync(userId, model.Password);
                if (addResult.Succeeded)
                {
                    return RedirectToAction("IzmeniProfil", "Admin");
                }
                AddErrorsFromResult(addResult);
            }
            // Korisnik vec ima lozinku
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff(string currentUrl)
        {
            AuthManager.SignOut();
            return Redirect(string.IsNullOrEmpty(currentUrl) ? "/" : currentUrl);
        }

        #region Alatke

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
        private const string XsrfKey = "XsrfId";
        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            { }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                ReturnUri = redirectUri;
                UserId = userId;
            }

            public string ReturnUri { get; set; }
            public string UserId { get; set; }
            public string LoginProvider { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                // base.ExecuteResult(context);

                var properties = new AuthenticationProperties { RedirectUri = ReturnUri };
                if (!string.IsNullOrEmpty(UserId))
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion Alatke
    }
}