using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Users.Infrastructure
{
    public class AuthorizeHelper : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Ako korisnik nema dovoljna prava pristupa
            // prebacice se na stranu sa porukom

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("/Admin/NotAuthorized");
                return;
            }

            filterContext.Result = new RedirectResult("/Account/Login");
        }
    }
}