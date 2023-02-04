using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ZemogaBlogAPI.Infrastructure.Auth
{
    public static class AuthorizationHandler
    {
        private static string aadInstance = "https://login.microsoftonline.com/{0}/v2.0";

        public static bool UserIsInAppRole(List<ClaimsIdentity> user, string[] validAppRoles)
        {
            var userRoles = user[0].Claims.Where(e => e.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(e => e.Value);
            var matchingRoles = userRoles.Intersect(validAppRoles);

            return matchingRoles.Count() > 0;
        }

        public static async Task<List<ClaimsIdentity>> ValidateRequestAsync(HttpRequest req, ILogger log)
        {
            var accessToken = GetAccessToken(req);
            return await ValidateAccessTokenAsync(accessToken, log);
        }

        private static async Task<List<ClaimsIdentity>> ValidateAccessTokenAsync(string accessToken, ILogger log)
        {
            var tenantId = Environment.GetEnvironmentVariable("TenantId");
            var clientId = Environment.GetEnvironmentVariable("ClientId");
            var audience = Environment.GetEnvironmentVariable("AudienceURI");

            if (tenantId == null)
            {
                tenantId = "common";
            }

            var authority = GetAuthority(tenantId);
            var validIssuers = GetValidIssuers(tenantId);

            var configManager =
                new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{authority}/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever());

            var config = await configManager.GetConfigurationAsync();
            var tokenValidator = new JwtSecurityTokenHandler();
            
            var validationParameters = new TokenValidationParameters
            {
                ValidAudiences = new[] { audience, clientId },
                ValidIssuers = tenantId == "common" ? null : validIssuers,
                //ValidateIssuer = tenantId != "common",
                IssuerSigningKeys = config.SigningKeys,
            };

            try
            {
                SecurityToken securityToken;
                //var tokenValidationResult = tokenValidator.ValidateToken(accessToken, validationParameters, out securityToken);

                var claims = new List<Claim>();

                foreach (var access in accessToken.Split(","))
                    claims.Add(new Claim(ClaimTypes.Role, access));

                return new List<ClaimsIdentity>() { new ClaimsIdentity(claims) };
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }

            return null;
        }

        private static string GetAccessToken(HttpRequest req)
        {
            string userAccess = string.Empty;

            var authorizationHeader = req.Headers?["Authorization"];
            var parts = authorizationHeader?.ToString().Split(null) ?? new string[0];

            if (parts.Length != 2 || !parts[0].Equals("Bearer"))
                throw new UnauthorizedException();

            var rolesHeader = req.Headers?["Roles"];
            var roles = rolesHeader?.ToString().Split(null) ?? new string[0];

            if(roles == null)
                throw new UnauthorizedException();

            foreach (var role in roles)
                userAccess += role;

            return userAccess;
        }

        private static string GetAuthority(string tenant) => string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        private static string[] GetValidIssuers(string tenant) => new string[]
        {
            $"https://login.microsoftonline.com/{tenant}/",
            $"https://login.microsoftonline.com/{tenant}/v2.0",
            $"https://login.windows.net/{tenant}/",
            $"https://login.microsoft.com/{tenant}/",
            $"https://sts.windows.net/{tenant}/"
        };
    }
}
