using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogService.Models
{
    public class BlogPostModel
    {
        public string Title { get; set; }
        public string Description { get; set; }     

    }
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }     

    }
}
