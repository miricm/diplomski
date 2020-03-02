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
    public class PostController : Controller
    {
        // GET: Post
        public ActionResult LoadPost(int postId = -1)
        {
            var article = Context.Articles.FindById(postId);

            if(article == null)
            {
                return Redirect("/");
            }

            ViewBag.Title = article.Title;
            return View(article);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PostComment(int postId, string userId, string text)
        {
            // Promeniti u AJAX?   
            // text: tekst komentara

            var article = Context.Articles.FindById(postId);
            var user = await UserManager.FindByIdAsync(userId);

            if (string.IsNullOrEmpty(text) || user == null)
            {
                TempData["EmptyCommentError"] = "Tekst komentara je obavezan!";
                return RedirectToAction("LoadPost", new { postId = article.Id });
            }

            Comment comment = new Comment
            {
                Article = article,
                DatePublished = DateTime.Now,
                PostedBy = user,
                Text = text
            };

            Context.Comments.Add(comment);
            await Context.SaveChangesAsync();

            return RedirectToAction("LoadPost", new { postId = article.Id });
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