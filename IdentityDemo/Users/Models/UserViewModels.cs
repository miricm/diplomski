using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using Users.Infrastructure;

namespace Users.Models
{
    // Account
    public class RegisterModel
    {
        [Required(ErrorMessage = "Korisnicko ime je obavezno!")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "E-Adresa je obavezna!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna!")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Korisnicko ime je obavezno!")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna!")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Verifikacioni kod")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Zapamti pretraživač?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    // Admin
    public class ArticleModel
    {
        [Required(ErrorMessage = "Naslov artikla je obavezan!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Kategorija artikla je obavezna!")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Artikal ne sme biti prazan!")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Naslovna slika je obavezna!")]
        public HttpPostedFileBase Image { get; set; }
    }

    public class AccountModifyModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        /// <summary>
        ///     FileBase format
        /// </summary>
        public HttpPostedFileBase NewProfilePicture { get; set; }

        /// <summary>
        ///     Byte array format
        /// </summary>
        [ReadOnly(true)]
        public byte[] CurrentProfilePicture { get; set; }

        [Required(ErrorMessage = "Korisnicko ime je obavezno!")]
        public string UserName { get; set; }

        [ReadOnly(true)]
        public DateTime DateRegistered { get; set; }

        [Required(ErrorMessage = "E-Adresa je obavezna!")]
        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail format neispravan!")]
        public string Email { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }

    public class RoleModifyModel
    {
        [ReadOnly(true)]
        public string UserId { get; set; }
        [ReadOnly(true)]
        public string UserName { get; set; }
        public IEnumerable<string> AllRoles { get; set; }
        public Dictionary<string, bool> IsInRole { get; set; }
    }

    // Security
    public class SecurityViewModel
    {
        public IEnumerable<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool BrowserRemembered { get; set; }        
    }
}