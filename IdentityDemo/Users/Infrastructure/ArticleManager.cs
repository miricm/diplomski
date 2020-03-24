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
            var articles = DbContext.Articles.Where(a => a.Id == articleId);
            return articles.Count() == 0 ? null : articles.FirstOrDefault();
        }

        public static IEnumerable<Article> FindByCategory(string value)
        {
            return DbContext.Articles.Where(a => a.Category == value).ToList();
        }

        public static IEnumerable<Article> GetArticles
        {
            get => DbContext.Articles;
        }

        public static IEnumerable<Article> GetArticlesForUser(string userId)
        {
            return DbContext.Articles.Where(a => a.Author.Id == userId).ToList();
        }

        public static async Task<bool> DeleteAsync(Article article)
        {
            try
            {
                var articleComments = DbContext.Comments.Where(c => c.Article.Id == article.Id);
                DbContext.Comments.RemoveRange(articleComments);
                DbContext.Articles.Remove(article);                
                await DbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // throw ex;
                return false;
            }
        }

        private static AppIdentityDbContext DbContext
        {
            get => HttpContext.Current.GetOwinContext().Get<AppIdentityDbContext>();
        }
    }
}