using BlogService.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BlogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateBlogPost([FromBody] BlogPostModel blog_post)
        {


            //return BadRequest();

            return CreatedAtAction("Blog Created", blog_post.Title);

        }



        [HttpGet("{id}")]
        public string[] GetBlog(string id)
        {
            return new string [] { id};
        }




    }
}
