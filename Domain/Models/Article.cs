using System;
using System.Collections.Generic;

namespace Users.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Text { get; set; }
        public byte[] Image { get; set; }
        public DateTime DatePublished { get; set; }

        public virtual AppUser Author { get; set; }
        public virtual List<Comment> Comments { get; set; }

    }
}