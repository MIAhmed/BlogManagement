using BlogService.Handlers.Commands;
using BlogService.Handlers.Queries;
using BlogService.Models;
using BlogService.Models.Commands;
using BlogService.Models.Queries;
using CachingLayer;
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
        private readonly RedisCacheService _cacheService;
        private static string _cachePrefix = "Blog_";
        private static readonly TimeSpan _cacheExpirationTimeMinutes = TimeSpan.FromMinutes(15);

        public BlogController(BlogCommandHandler commandHandler, BlogQueryHandler queryHandler, RedisCacheService cacheService) {
            _commandHandler = commandHandler;
            _queryHandler = queryHandler;
            _cacheService = cacheService;
        }



        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromBody] CreateBlogCommand command)
        {

            try
            {
                if (string.IsNullOrEmpty(command.Title))
                {
                    return BadRequest("Title is required");
                }

                if (string.IsNullOrEmpty(command.Content))
                {
                    return BadRequest("Content is required");
                }



                int createdPostId = await _commandHandler.Process(command);

                if (createdPostId <= 0)
                {
                    return StatusCode(500, "Error while creating blog");
                }

                return CreatedAtAction("GetBlog", new { id = createdPostId }, command);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error while creating blog: {ex.Message}");

            }
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlog(string id)
        {
            
            int number_id;
            try
            {
                bool number_found = int.TryParse(id, out number_id);

                // validation of the provided id
                if (!number_found)
                {
                    return BadRequest("Blog id should be a valid number");
                }

                // creating key for the cache
                var cacheKey = $"{_cachePrefix}{id}";

                // getting data from cache based on the formated key that contains id of the blog
                var cachedData = await _cacheService.GetAsync<DatabaseLayer.Models.BlogPost>(cacheKey);

                // If item found from the cache return it as response instead of checking database
                if (cachedData != null)
                {
                    return Ok(cachedData);
                }

                // Item not found in the cache so now getting it from the SQL Server
                var query = new GetBlogQuery { Id = number_id };
                var blogData = await _queryHandler.Process(query);

                if (blogData == null)
                {
                    // Item not found in the database
                    return NotFound();
                }

                //Item found in datbase now saving it to the cache
                await _cacheService.SetAsync<DatabaseLayer.Models.BlogPost>(cacheKey, blogData, _cacheExpirationTimeMinutes);


                return Ok(blogData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error while getting blog: {ex.Message}");

            }

        }




    }
}
