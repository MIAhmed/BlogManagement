using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AuthenticationLayer
{
    public class JwtAuthenticationHandler : AuthenticationHandler<JwtAuthenticationOptions>
    {
        private readonly AuthService _authService;

        public JwtAuthenticationHandler(
            IOptionsMonitor<JwtAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            AuthService authService)
            : base(options, logger, encoder, clock)
        {
            _authService = authService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();

            var authorizationHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                return AuthenticateResult.NoResult();

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
                return AuthenticateResult.Fail("Token is missing or empty.");

            var principal = await Task.Run(() => _authService.ValidateToken(token));

            if (principal == null)
                return AuthenticateResult.Fail("Invalid or expired token.");

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }

    public class JwtAuthenticationOptions : AuthenticationSchemeOptions
    {
        
    }

    public static class JwtAuthenticationDefaults
    {
        public const string AuthenticationScheme = "JwtBearer";
    }
}
