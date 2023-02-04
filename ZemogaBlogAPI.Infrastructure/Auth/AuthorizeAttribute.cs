using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading;
using System.Threading.Tasks;

namespace ZemogaBlogAPI.Infrastructure.Auth
{
    public class AuthorizeAttribute : FunctionInvocationFilterAttribute
    {
        private string[] _validAppRoles;

        public AuthorizeAttribute(params string[] validAppRoles)
        {
            _validAppRoles = validAppRoles;
        }

        public override async Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
                var request = (HttpRequest)executingContext.Arguments["req"];
            try
            {
                var user = await AuthorizationHandler.ValidateRequestAsync(request, executingContext.Logger);
                var userIsAuthorized = user != null &&
                                        (_validAppRoles.Length == 0 ||
                                         AuthorizationHandler.UserIsInAppRole(user, _validAppRoles));

                if (userIsAuthorized)
                {
                    request.HttpContext.User.AddIdentities(user);
                }
                else
                {
                    request.HttpContext.Response.StatusCode = 401;
                    await request.HttpContext.Response.Body.FlushAsync();
                    request.HttpContext.Response.Body.Close();
                    throw new UnauthorizedException();
                }
            }
            catch (UnauthorizedException e)
            {
                request.HttpContext.Response.StatusCode = 401;
                await request.HttpContext.Response.Body.FlushAsync();
                request.HttpContext.Response.Body.Close();
                throw e;
            }
        }
    }
}
