using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using Users.Infrastructure;
using Users.Models;

namespace IdentityDemo.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var model = Context.Articles.OrderByDescending(a => a.DatePublished)
                                        .Take(4)
                                        .ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> UcitajProfilnuSliku()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user != null)
            {
                return Json(Convert.ToBase64String(user.ProfilePicture));
            }
            // Nalog nije pronadjen, NE OTKRIVATI KORISNIKU
            return View("HomeErrorPage", (object)"Došlo je do greške, pokušajte ponovo.");
        }

        // Alatke
        private AppIdentityDbContext Context
        {
            get => HttpContext.GetOwinContext().Get<AppIdentityDbContext>();
        }
        private AppUserManager UserManager
        {
            get => HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
        }
    }
}