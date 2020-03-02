using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Users.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime DatePublished { get; set; }

        public virtual AppUser PostedBy { get; set; }
        public virtual Article Article { get; set; }
    }
}