using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
    [RoutePrefix("Artikli")]
    public class PostController : Controller
    {
        // GET: Post
        [Route("{articleId}")]
        public ActionResult LoadPost(int articleId = -1)
        {
            // var article = Context.Articles.FindById(articleId);
            var article = ArticleManager.FindById(articleId);

            if(article == null)
            {
                return Redirect("/");
            }

            ViewBag.Title = article.Title;
            return View(article);
        }

        [HttpPost]
        [Authorize]
        [Route("{articleId}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PostComment(AddCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ValidationError"] = "Niste uneli komentar!";
                return Redirect($"/Artikli/{model.ArticleId}");
            }

            var article = ArticleManager.FindById(model.ArticleId);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (article != null && user != null)
            {
                Comment comment = new Comment
                {
                    Article = article,
                    DatePublished = DateTime.Now,
                    PostedBy = user,
                    Text = model.Text
                };

                await ArticleManager.CreateCommentAsync(comment);
                return Redirect($"/Artikli/{model.ArticleId}");
            }
            // Korisnik ili artikal nisu pronadjeni
            return View("Error");
        }

        [Route("Kategorije/{kategorija}")]
        public ViewResult Kategorije(string kategorija)
        {
            ViewBag.Title = kategorija;
            var model = ArticleManager.FindByCategory(kategorija);
            return View(model);
        }

        #region Alatke
        private AppIdentityDbContext Context
        {
            get => HttpContext.GetOwinContext().Get<AppIdentityDbContext>();
        }
        private AppUserManager UserManager
        {
            get => HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
        }
        #endregion Alatke
    }
}