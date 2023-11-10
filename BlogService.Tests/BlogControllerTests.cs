using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AuthenticationLayer;
using BlogService.Controllers;
using BlogService.Handlers.Commands;
using BlogService.Handlers.Queries;
using BlogService.Models.Commands;
using CachingLayer;
using DatabaseLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BlogService.Tests
{
    public class BlogControllerTests
    {
      

        [Fact]
        public async Task CreateBlog_ValidInput_ReturnsCreatedResult()
        {
            // Arrange
            // Set up an in-memory database for testing
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new AppDbContext(dbContextOptions);

            var commandHandlerMock = new Mock<BlogCommandHandler>(dbContext);
            var queryHandlerMock = new Mock<BlogQueryHandler>(dbContext);

            var cacheServiceMock = new Mock<TestInMemoryCacheService>();

            //// Set up the expected behavior for GetAsync
            //cacheServiceMock.Setup(x => x.GetAsync<DatabaseLayer.Models.BlogPost>(It.IsAny<string>()))
            //    .ReturnsAsync(new DatabaseLayer.Models.BlogPost { /* Set your expected data here */ });


            var authServiceMock = new Mock<TestAuthService>("dca94f3b5b9978653e614e019890db22", 300);

            var blogController = new BlogController(
                commandHandlerMock.Object,
                queryHandlerMock.Object,
                cacheServiceMock.Object,
                authServiceMock.Object
            );

            var createBlogCommand = new CreateBlogCommand
            {
                Title = "Test Title",
                Content = "Test Content",
                // Add other properties as needed
            };

           

            
            // Set the ControllerContext on the controller
            blogController.ControllerContext = GetHttpMockContext();

            // Act
            var resultWithToken = await blogController.CreateBlog(createBlogCommand) as CreatedAtActionResult;

            Assert.Equal(201, resultWithToken.StatusCode);

        }

        [Fact]
        public async Task CreateBlog_MissingTitle_ReturnsBadRequest()
        {
            // Arrange
            // Set up an in-memory database for testing
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new AppDbContext(dbContextOptions);

            var commandHandlerMock = new Mock<BlogCommandHandler>(dbContext);
            var queryHandlerMock = new Mock<BlogQueryHandler>(dbContext);

            var cacheServiceMock = new Mock<TestInMemoryCacheService>();

            var authServiceMock = new Mock<TestAuthService>("dca94f3b5b9978653e614e019890db22", 300);

            var blogController = new BlogController(
                commandHandlerMock.Object,
                queryHandlerMock.Object,
                cacheServiceMock.Object,
                authServiceMock.Object
            );
            var createBlogCommand = new CreateBlogCommand
            {
                // Missing title intentionally
                Content = "Test Content"
            };
            // Set the ControllerContext on the controller
            blogController.ControllerContext = GetHttpMockContext();

            // Act

            var result = await blogController.CreateBlog(createBlogCommand) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Title is required", result.Value);

            // Additional assertions based on your specific implementation
        }

        [Fact]
        public async Task CreateBlog_MissingContent_ReturnsBadRequest()
        {
            // Arrange
            // Set up an in-memory database for testing
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new AppDbContext(dbContextOptions);

            var commandHandlerMock = new Mock<BlogCommandHandler>(dbContext);
            var queryHandlerMock = new Mock<BlogQueryHandler>(dbContext);

            var cacheServiceMock = new Mock<TestInMemoryCacheService>();

            var authServiceMock = new Mock<TestAuthService>("dca94f3b5b9978653e614e019890db22", 300);

            var blogController = new BlogController(
                commandHandlerMock.Object,
                queryHandlerMock.Object,
                cacheServiceMock.Object,
                authServiceMock.Object
            );
            var createBlogCommand = new CreateBlogCommand
            {
                Title = "Test Title",
                // Missing content intentionally
            };

            // Set the ControllerContext on the controller
            blogController.ControllerContext = GetHttpMockContext();

            // Act
            var result = await blogController.CreateBlog(createBlogCommand) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Content is required", result.Value);

        }

        #region Get Blogs

        [Fact]
        public async Task GetBlog_ValidId_CacheHit_ReturnsOkResult()
        {
            // Set up an in-memory database for testing
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new AppDbContext(dbContextOptions);

            var commandHandlerMock = new Mock<BlogCommandHandler>(dbContext);
            var queryHandlerMock = new Mock<BlogQueryHandler>(dbContext);

            var cacheServiceMock = new Mock<TestInMemoryCacheService>();

            var authServiceMock = new Mock<TestAuthService>("dca94f3b5b9978653e614e019890db22", 300);

            // Set up the expected behavior for GetAsync to simulate a cache hit
            var expectedBlogPost = new DatabaseLayer.Models.BlogPost { Id = 1, Title = "Test Title", Content = "Test Content" };
            var cacheKey = $"Posts_{expectedBlogPost.Id}";

            cacheServiceMock.Setup(x => x.GetAsync<DatabaseLayer.Models.BlogPost>(cacheKey))
                .ReturnsAsync(expectedBlogPost);

            var blogController = new BlogController(
                commandHandlerMock.Object,
                queryHandlerMock.Object,
                cacheServiceMock.Object,
                authServiceMock.Object
            );


            // Set the ControllerContext on the controller
            blogController.ControllerContext = GetHttpMockContext();

            // Act
            var result = await blogController.GetBlog(expectedBlogPost.Id.ToString()) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(expectedBlogPost, result.Value);
        }


        [Fact]
        public async Task GetBlog_ValidId_CacheMiss_DbHit_ReturnsOkResult()
        {
            // Arrange
            // Set up an in-memory database for testing
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new AppDbContext(dbContextOptions);

            var commandHandlerMock = new Mock<BlogCommandHandler>(dbContext);
            var queryHandlerMock = new Mock<BlogQueryHandler>(dbContext);

            var cacheServiceMock = new Mock<TestInMemoryCacheService>();

            var authServiceMock = new Mock<TestAuthService>("dca94f3b5b9978653e614e019890db22", 300);

            // Set up the expected behavior for GetAsync to simulate a cache hit
            var expectedBlogPost = new DatabaseLayer.Models.BlogPost { Id = 1, Title = "Test Title", Content = "Test Content" };
            var cacheKey = $"Posts_{expectedBlogPost.Id}";

            cacheServiceMock.Setup(x => x.GetAsync<DatabaseLayer.Models.BlogPost>(cacheKey))
                .ReturnsAsync(expectedBlogPost);

            var blogController = new BlogController(
                commandHandlerMock.Object,
                queryHandlerMock.Object,
                cacheServiceMock.Object,
                authServiceMock.Object
            );

            

            // Set up the expected behavior for GetAsync to simulate a cache miss
            cacheServiceMock.Setup(x => x.GetAsync<DatabaseLayer.Models.BlogPost>(cacheKey))
                .ReturnsAsync((DatabaseLayer.Models.BlogPost)null);

            // Set up the expected behavior for QueryHandler to simulate a database hit
            await commandHandlerMock.Object.Process(new CreateBlogCommand() { Content = expectedBlogPost.Content, Title = expectedBlogPost.Title});
            //queryHandlerMock.Setup(x => x.Process(It.IsAny<Models.Queries.GetBlogQuery>()))
            //    .ReturnsAsync(expectedBlogPost);

            // Set the ControllerContext on the controller
            blogController.ControllerContext = GetHttpMockContext();
            // Act
            var result = await blogController.GetBlog(expectedBlogPost.Id.ToString()) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(expectedBlogPost.Id, (result.Value as DatabaseLayer.Models.BlogPost).Id);

          
        }

        [Fact]
        public async Task GetBlog_InvalidId_ReturnsBadRequestResult()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new AppDbContext(dbContextOptions);

            var commandHandlerMock = new Mock<BlogCommandHandler>(dbContext);
            var queryHandlerMock = new Mock<BlogQueryHandler>(dbContext);

            var cacheServiceMock = new Mock<TestInMemoryCacheService>();

            var authServiceMock = new Mock<TestAuthService>("dca94f3b5b9978653e614e019890db22", 300);

         
            var blogController = new BlogController(
                commandHandlerMock.Object,
                queryHandlerMock.Object,
                cacheServiceMock.Object,
                authServiceMock.Object
            );

            var invalidBlogId = "invalid";
            // Set the ControllerContext on the controller
            blogController.ControllerContext = GetHttpMockContext();
            // Act
            var result = await blogController.GetBlog(invalidBlogId) as BadRequestObjectResult;

            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Blog id should be a valid number", result.Value);
        }


        [Fact]
        public async Task GetBlog_ValidId_DbMiss_ReturnsNotFoundResult()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new AppDbContext(dbContextOptions);

            var commandHandlerMock = new Mock<BlogCommandHandler>(dbContext);
            var queryHandlerMock = new Mock<BlogQueryHandler>(dbContext);

            var cacheServiceMock = new Mock<TestInMemoryCacheService>();

            var authServiceMock = new Mock<TestAuthService>("dca94f3b5b9978653e614e019890db22", 300);


            var blogController = new BlogController(
                commandHandlerMock.Object,
                queryHandlerMock.Object,
                cacheServiceMock.Object,
                authServiceMock.Object
            );

            var blogId = "250";
            var cacheKey = $"Posts_{blogId}";

            // Set up the expected behavior for GetAsync to simulate a cache miss
            cacheServiceMock.Setup(x => x.GetAsync<DatabaseLayer.Models.BlogPost>(cacheKey))
                .ReturnsAsync((DatabaseLayer.Models.BlogPost)null);

            // Set the ControllerContext on the controller
            blogController.ControllerContext = GetHttpMockContext();
            // Act
            var result = await blogController.GetBlog(blogId) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);

           
        }

        #endregion

        private ControllerContext GetHttpMockContext()
        {
            // Create a ControllerContext with a mock HttpContext
            var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            httpContextMock.SetupGet(c => c.Request.Headers["Authorization"]).Returns("Bearer your_valid_bearer_token");

            // Set a fake user for the test
            var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "123") };
            var fakeUserIdentity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuthentication");
            httpContextMock.SetupGet(c => c.User).Returns(new System.Security.Claims.ClaimsPrincipal(fakeUserIdentity));

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object,
            };
            return controllerContext;
        }

    }


    // To use in memory cache that mimics same as the Redis cache
    public class TestInMemoryCacheService : ICacheService
    {
        private readonly Dictionary<string, string> _inMemoryCache = new Dictionary<string, string>();

        public TestInMemoryCacheService() 
        {        }

        public virtual async Task<T> GetAsync<T>(string key)
        {
            if (_inMemoryCache.TryGetValue(key, out var cachedData))
            {
                return JsonSerializer.Deserialize<T>(cachedData);
            }

            return default;
        }

        public virtual async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var data = JsonSerializer.Serialize(value);
            _inMemoryCache[key] = data;
        }

        public async Task RemoveAsync<T>(string key)
        {
            _inMemoryCache.Remove(key);
        }
    }
}
