using BlogService.Handlers.Commands;
using BlogService.Handlers.Queries;
using BlogService.Models;
using BlogService.Models.Commands;
using BlogService.Models.Queries;
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

        private readonly BlogCommandHandler _commandHandler;
        private readonly BlogQueryHandler  _queryHandler;

        public BlogController(BlogCommandHandler commandHandler, BlogQueryHandler queryHandler) {
            _commandHandler = commandHandler;
            _queryHandler = queryHandler;
        }



        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromBody] CreateBlogCommand command)
        {

            try
            {
                int createdPostId = await _commandHandler.Process(command);
                return CreatedAtAction("GetBlog", new { id = createdPostId }, command);
            }
            catch (Exception ex)
            {
                return BadRequest();

            }
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlog(int id)
        {
            var query = new GetBlogQuery { Id = id };
            var blogData = await _queryHandler.Process(query);

            if (blogData == null)
            {
                return NotFound();
            }

            return Ok(blogData);


        }




    }
}
