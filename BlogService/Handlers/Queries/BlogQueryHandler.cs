using BlogService.Models.Queries;
using DatabaseLayer;
using DatabaseLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace BlogService.Handlers.Queries
{
    public class BlogQueryHandler
    {
        private readonly AppDbContext _dbContext;

        public BlogQueryHandler(AppDbContext context)
        {
            _dbContext = context;
        }



        public async Task<BlogPost> Process(GetBlogQuery query)
        {
            return await _dbContext.BlogPosts
                .Include(x => x.Comments)
                .FirstOrDefaultAsync(x => x.Id == query.Id);

        }
    }
}
