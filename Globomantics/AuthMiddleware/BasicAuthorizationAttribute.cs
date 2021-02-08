using Microsoft.AspNetCore.Authorization;

namespace Globomantics.AuthMiddleware
{
    public class BasicAuthorizationAttribute : AuthorizeAttribute
    {
        public BasicAuthorizationAttribute()
        {
            Policy = "BasicAuthentication";
        }
    }
}
