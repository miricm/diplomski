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
    public class AccountController : Controller
    {
        // currentUrl = Url sa kojeg je pozvana akcija

        public ActionResult Register(string returnUrl, string currentUrl)
        {
            ViewBag.ReturnUrl = returnUrl == null ? currentUrl : returnUrl;
            return View();
        }

        [HttpPost]
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

                var resultCreate   = await UserManager.CreateAsync(user, model.Password);                

                if (resultCreate.Succeeded)
                {
                    var resultAddRole  = await UserManager.AddToRoleAsync(user.Id, "Korisnik");
                    if(resultAddRole.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: model.RememberMe, rememberBrowser: false);
                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        return Redirect(returnUrl);
                    }
                    // Dodavanje uloge nije uspelo
                    AddErrorsFromResult(resultAddRole);
                }
                // Kreiranje naloga nije uspelo
                AddErrorsFromResult(resultCreate);
            }
            // Model ne valja, prikazi pogled ponovo
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        public ActionResult Login(string returnUrl, string currentUrl)
        {
            ViewBag.ReturnUrl = returnUrl == null ? currentUrl : returnUrl.Replace("PostComment", "LoadPost");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel details, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await UserManager.FindAsync(details.UserName, details.Password);

                if (user == null)
                {
                    ModelState.AddModelError("", "Korisničko ime ili lozinka nisu ispravni!");
                }
                else
                {
                    //ClaimsIdentity identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                    //AuthManager.SignOut();
                    //AuthManager.SignIn(new AuthenticationProperties { IsPersistent = true }, identity);

                    await SignInManager.SignInAsync(user, isPersistent: details.RememberMe, rememberBrowser: false);
                    
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect("/");
                    }
                    return Redirect(returnUrl);
                }
            }
            // Model nije dobar, vrati pogled
            ViewBag.ReturnUrl = returnUrl;
            return View(details);
        }
        
        [Authorize]
        public ActionResult LogOff(string currentUrl)
        {
            AuthManager.SignOut();

            if (string.IsNullOrEmpty(currentUrl))
            {
                return Redirect("/");
            }
            return Redirect(currentUrl);
        }


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