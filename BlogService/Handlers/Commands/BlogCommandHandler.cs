using BlogService.Models.Commands;
using DatabaseLayer;
using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogService.Handlers.Commands
{
    
    public class BlogCommandHandler
    {
        private readonly AppDbContext _dbContext;

        public BlogCommandHandler(AppDbContext context)
        {
            _dbContext = context;
        }



        public async Task<int> Process(CreateBlogCommand cmd)
        {
            var blog = new BlogPost
            {
                Title = cmd.Title,
                Content = cmd.Content
            };

            _dbContext.BlogPosts.Add(blog);
            await _dbContext.SaveChangesAsync();
            return blog.Id;

        }




    }
}
