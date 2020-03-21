using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Users.Infrastructure;
using Users.Models;
using Newtonsoft.Json;
using System;

namespace IdentityDemo.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        // GET: Admin

        public ActionResult Index()
        {
            AppUser user = UserManager.FindById(User.Identity.GetUserId());
            return View(user);
        }

        public async Task<ActionResult> IzmeniProfil()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if(user == null)
            {
                // Ne otkrivaj da korisnik ne postoji
                return View("Error");
            }

            var model = new AccountModifyModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateRegistered = user.DateRegistered,
                CurrentProfilePicture = user.ProfilePicture,
                HasPassword = await UserManager.HasPasswordAsync(user.Id)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> IzmeniProfil(AccountModifyModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    user.FirstName  = model.FirstName;
                    user.LastName   = model.LastName;
                    user.UserName   = model.UserName;
                    
                    if(model.NewProfilePicture != null)
                    {
                        user.ProfilePicture = model.NewProfilePicture.ToByteArray();
                    }

                    var updateResult       = await UserManager.UpdateAsync(user);
                    var emailUpdateResult  = await UserManager.SetEmailAsync(user.Id, model.Email);

                    if (updateResult.Succeeded && emailUpdateResult.Succeeded)
                    {
                        AuthManager.SignOut();
                        await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false);

                        TempData["Success"] = "Izmene uspesno sacuvane!";
                        return RedirectToAction("IzmeniProfil");
                    }

                    AddErrorsFromResult(string.Empty, updateResult, emailUpdateResult);
                }
                ModelState.AddModelError(string.Empty, "Doslo je do greske, pokušajte ponovo.");
            }
            return View(model);
        }

        [AuthorizeHelper(Roles = "Administrator")]
        public ViewResult Uloge()
        {
            return View(RoleManager.Roles);
        }

        [AuthorizeHelper(Roles = "Administrator")]
        public async Task<ViewResult> Nalozi()
        {
            var users = UserManager.Users;
            StringBuilder roleNames = new StringBuilder();
            foreach (var user in users)
            {
                // Radi prikaza na tabeli
                var roles = await UserManager.GetRolesAsync(user.Id);
                foreach (var role in roles)
                {
                    roleNames.Append(role + " ");
                }
                user.RoleNames = roleNames.ToString();
                roleNames.Clear();
            }
            return View(users);
        }

        [AuthorizeHelper(Roles = "Autor")]
        public ViewResult Objave()
        {
            var articles = ArticleManager.GetArticlesForUser(User.Identity.GetUserId());
            return View(new UserPostsModel { Articles = articles });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ObrisiObjavu(int postId = -1)
        {
            var article = ArticleManager.FindById(postId);
            if(article == null)
            {
                // Ne otkrivati da ne postoji
                return View("AdminErrorPage");
            }

            if(article.Author.Id == User.Identity.GetUserId())
            {
                if (await ArticleManager.DeleteAsync(article))
                {
                    return RedirectToAction("Objave");
                }                
            }
            // Doslo je do modifikacije id-a objave, ili nije uspelo brisanje
            return View("AdminErrorPage");
        }

        [AuthorizeHelper(Roles = "Autor, Moderator, Administrator")]
        public ViewResult Objavi()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Moze se zabraniti preko [Authorize]
        public async Task<ActionResult> Objavi(ArticleModel articleModel, string userId)
        {
            if (ModelState.IsValid)
            {
                byte[] img = articleModel.Image.ToByteArray();
                AppUser author = await UserManager.FindByIdAsync(userId);

                if(string.IsNullOrEmpty(author.FirstName) || string.IsNullOrEmpty(author.LastName))
                {
                    ModelState.AddModelError("", "Morate ažurirati ime ili prezime da biste objavili artikal!");
                    return View(articleModel);
                }

                Article article = new Article
                {
                    Title = articleModel.Title,
                    Text = articleModel.Text,
                    Image = img,
                    Author = author,
                    Category = articleModel.Category,
                    DatePublished = DateTime.Now
                };

                //Context.Articles.Add(article);
                //await Context.SaveChangesAsync();
                await ArticleManager.CreateArticleAsync(article);
                ViewBag.PostSuccess = "Artikal uspešno objavljen!";
            }
            return View(articleModel);
        }

        [AuthorizeHelper(Roles = "Administrator")]
        public async Task<ActionResult> IzmeniPrava(string userId)
        {
            // userID : ID korisnikia cija prava menja admin
            // Pitati profesora
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                var allRoles = RoleManager.Roles.Select(role => role.Name);
                var model = new RoleModifyModel
                {
                    UserId   = user.Id,
                    UserName = user.UserName,
                    IsInRole = new Dictionary<string, bool>(),
                    AllRoles = allRoles
                };

                foreach (var roleName in allRoles)
                {
                    bool isInRole = await UserManager.IsInRoleAsync(user.Id, roleName);
                    model.IsInRole.Add(key: roleName, value: isInRole);
                }

                return View(model);
            }
            return View("AdminErrorPage");
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> IzmeniPrava(string userName, string newRoles)
        {
            var user = await UserManager.FindByNameAsync(userName);

            if (user != null)
            {
                var userRoles = await UserManager.GetRolesAsync(user.Id);
                var rolesToAdd = JsonConvert.DeserializeObject<IEnumerable<string>>(newRoles);

                // Ukloni nalog iz svih uloga
                var removeResult = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.ToArray());

                if (!removeResult.Succeeded)
                {
                    return Json(removeResult.Errors);
                }

                // Dodaj u nove uloge
                var addResult = await UserManager.AddToRolesAsync(user.Id, rolesToAdd.ToArray());

                if (!addResult.Succeeded)
                {
                    return Json(addResult.Errors);
                }

                // Sve se izvrsilo kako treba
                return Json("Uloge uspešno izmenjene.");
            }
            // Korisnik ne postoji
            return View("AdminErrorPage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PromeniLozinku(ChangePasswordModel model)
        {
            if (Request.IsAjaxRequest())
            {
                if (ModelState.IsValid)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    var result = await UserManager.ChangePasswordAsync(user.Id, model.CurrentPassword, model.NewPassword);

                    if (result.Succeeded)
                    {
                        ViewBag.ChangePassSuccess = "Lozinka promenjena!";
                        return PartialView("~/Views/Partial/_ChangePasswordPartial.cshtml", model);
                    }
                    AddErrorsFromResult("ChangePasswordValidation", result);
                }
                // Model nije dobar, vrati pogled
                return PartialView("~/Views/Partial/_ChangePasswordPartial.cshtml", model);
            }

            // Nije uspeo AJAX poziv, ispisi poruku
            return Json("Doslo je do greske u obradi zahteva, pokusajte kasnije.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ObrisiNalog(string userId)
        {
            AppUser user = await UserManager.FindByIdAsync(userId);

            if (user != null)
            {
                var result = await UserManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    if (User.Identity.GetUserId().Equals(user.Id))
                    {
                        // Ako korisnik brise svoj nalog, odjavi ga
                        AuthManager.SignOut();
                        return Redirect("/");
                    }
                    return RedirectToAction("Nalozi");
                }
                // Brisanje nije uspelo, ispisi greske
                AddErrorsFromResult("AccountDeleteValidation", result);
            }
            TempData["accNotFoundError"] = "Greska! Pokušajte kasnije.";
            return RedirectToAction("IzmeniProfil", new { userId });
        }

        public ViewResult NotAuthorized()
        {
            return View();
        }



        #region Alatke
        private AppIdentityDbContext Context
        {
            get => HttpContext.GetOwinContext().Get<AppIdentityDbContext>();
        }
        private AppRoleManager RoleManager
        {
            get => HttpContext.GetOwinContext().GetUserManager<AppRoleManager>();
        }
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
        private void AddErrorsFromResult(string key, params IdentityResult[] results)
        {
            foreach (var result in results)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(key, error);
                }
            }
        }
        #endregion Alatke
    }
}