using AuthenticationLayer;
using BlogService.Handlers.Commands;
using BlogService.Handlers.Queries;
using BlogService.Models;
using BlogService.Models.Commands;
using BlogService.Models.Queries;
using CachingLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BlogService.Controllers
{
    [Authorize] // will cause all the APIs in this class to have the valid tokens
    [Route("api/posts")]
    [ApiController]
    public class BlogController : ControllerBase
    {

        private readonly BlogCommandHandler _commandHandler;
        private readonly BlogQueryHandler  _queryHandler;
        private readonly RedisCacheService _cacheService;
        private readonly AuthService _authService;
        private static string _cachePrefix = "Posts_"; // prefix that will be used in all the Blog posts keys along with the posts ids
        private static readonly TimeSpan _cacheExpirationTimeMinutes = TimeSpan.FromMinutes(15);

        public BlogController(BlogCommandHandler commandHandler, BlogQueryHandler queryHandler, RedisCacheService cacheService, AuthService authService) {
            _commandHandler = commandHandler;
            _queryHandler = queryHandler;
            _cacheService = cacheService;
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromBody] CreateBlogCommand command)
        {
            try
            {
                // can use this user id to set blog post created by the user
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid or expired token");
                }

                // validating if the title was provided
                if (string.IsNullOrEmpty(command.Title))
                {
                    return BadRequest("Title is required");
                }

                // validating if the content was provided
                if (string.IsNullOrEmpty(command.Content))
                {
                    return BadRequest("Content is required");
                }


                //checking if the created record has a valid id, that indicates record added
                int createdPostId = await _commandHandler.Process(command);

                if (createdPostId <= 0)
                {
                    return StatusCode(500, "Error while creating blog");
                }

                // retuning the response that will point to the created record using the get blog API
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
                // can use this user id to set the stats if required like blog accessed by 
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid or expired token");
                }


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
