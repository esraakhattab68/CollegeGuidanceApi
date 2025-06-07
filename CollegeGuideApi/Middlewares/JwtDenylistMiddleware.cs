using CollegeGuideApi.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace CollegeGuideApi.Middlewares
{
    public class JwtDenylistMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtDenylistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var jtiClaim = context.User.FindFirst(JwtRegisteredClaimNames.Jti);
                if (jtiClaim != null && !string.IsNullOrWhiteSpace(jtiClaim.Value))
                {
                    if (await authService.IsTokenRevokedAsync(jtiClaim.Value))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                        await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked." });
                        //await context.Response.WriteAsync("Token has been revoked.");

                        return;
                    }
                }
            }
            await _next(context);
        }
    }
}
