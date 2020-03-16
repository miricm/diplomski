using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Users.Models;

namespace Users.Infrastructure
{
    public static class ArticleManager
    {
        public static async Task CreateArticleAsync(Article article)
        {
            DbContext.Articles.Add(article);
            await DbContext.SaveChangesAsync();
        }

        public static async Task CreateCommentAsync(Comment comment)
        {
            DbContext.Comments.Add(comment);
            await DbContext.SaveChangesAsync();
        }

        public static Article FindById(int articleId)
        {
            if(articleId == -1)
            {
                return null;
            }

            return DbContext.Articles.Where(a => a.Id == articleId).FirstOrDefault();
        }

        public static IEnumerable<Article> GetArticles
        {
            get => DbContext.Articles;
        }

        private static AppIdentityDbContext DbContext
        {
            get => HttpContext.Current.GetOwinContext().Get<AppIdentityDbContext>();
        }
    }
}