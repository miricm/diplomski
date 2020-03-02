using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Users.Models;

namespace Users.Infrastructure
{
    public static class CustomExtensionMethods
    {
        public static Article FindById(this IEnumerable<Article> articles, int articleId)
        {
            var target = articles.Where(a => a.Id == articleId);
            return target.Count() == 0 ? null : target.SingleOrDefault();
        }

        public static byte[] ToByteArray(this HttpPostedFileBase file)
        {
            if(file == null)
            {
                return null;
            }

            using (Stream inputStream = file.InputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }
                return memoryStream.ToArray();
            }
        }
    }
}