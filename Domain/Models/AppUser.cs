using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Users.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateRegistered { get; set; }
        public byte[] ProfilePicture { get; set; }

        [NotMapped]
        public string RoleNames { get; set; }

        public virtual List<Comment> Comments { get; set; }
        public virtual List<Article> Posts { get; set; }
    }
}