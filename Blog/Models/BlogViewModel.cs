using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class BlogViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
    }
}