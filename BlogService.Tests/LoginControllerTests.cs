using AuthenticationLayer;
using BlogService.Controllers;
using BlogService.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;
using Xunit;

namespace BlogService.Tests
{
    public class LoginControllerTests
    {
        public string Token { get; set; }
        [Fact]
        public string Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var authServiceMock = new Mock<TestAuthService>("dca94f3b5b9978653e614e019890db22" , 300);
            //authServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>())).Returns("mocked-token");

            var loginController = new LoginController(authServiceMock.Object);
            var loginModel = new LoginModel { UserName = "demo", Password = "demo" };

            // Act
            var result = loginController.Login(loginModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            // Access the token property
            string tokenValue = result.Value.ToString().Substring(10, result.Value.ToString().Length - 10 - 2);
            Assert.True(tokenValue.Length > 150);

            return tokenValue;
        }

        [Fact]
        public void Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>("dca94f3b5b9978653e614e019890db22", 300);
            var loginController = new LoginController(authServiceMock.Object);
            var loginModel = new LoginModel { UserName = "invalid", Password = "invalid" };

            // Act
            var result = loginController.Login(loginModel) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
            Assert.Equal("Invalid credentials", result.Value);
        }
    }


    public class TestAuthService : AuthService
    {
        public TestAuthService(string secretKey, int jwtTokenExpiryInMinutes)
            : base(secretKey, jwtTokenExpiryInMinutes)
        {
            
        }

        public virtual new string GenerateToken(string userData)
        {
            // Implement your mock behavior here
            return "mocked-token";
        }
    }

    class TokenResponse
    {
        
        public string token { get; set; }
    }

}
