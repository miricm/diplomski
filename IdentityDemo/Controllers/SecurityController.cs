﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Users.Infrastructure;

namespace IdentityDemo.Controllers
{
    [Authorize]
    public class SecurityController : Controller
    {
        // GET: Security
        public ActionResult Index()
        {
            return Redirect("/Admin/Sigurnost");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuth()
        {
            // Omoguci 2FA
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var isPersistant = ((System.Web.Security.FormsIdentity)User.Identity).Ticket.IsPersistent;

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
    }
}