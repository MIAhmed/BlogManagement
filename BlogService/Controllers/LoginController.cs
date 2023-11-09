using AuthenticationLayer;
using BlogService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogService.Controllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AuthService _authService;
       private Random _random = new Random(); // just to generate random user ids

        public LoginController(AuthService authService)
        {
            _authService = authService;
        }


        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            // here should be acutal username and the password should be validated, but here doing as fixed user credentials
            if (model.UserName == "demo" && model.Password == "demo")
            {
                // should get the id of the authenticated user
                // here for the demo purpose generating random user id to simulate same expereince
                var userId = _random.Next(1, 9999);

                // generating and retuning the token to the user
                var token = _authService.GenerateToken(userId.ToString());
                return Ok(new { token });
            }

            return Unauthorized("Invalid credentials");
        }
    }
}
