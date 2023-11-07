using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogService.Models.Commands
{
    public class CreateBlogCommand
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
