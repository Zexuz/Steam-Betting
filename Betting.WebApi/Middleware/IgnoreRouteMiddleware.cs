using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Betting.WebApi.Middleware
{
    public class IgnoreRouteMiddleware
    {
        private readonly RequestDelegate next;

        // You can inject a dependency here that gives you access
        // to your ignored route configuration.
        public IgnoreRouteMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue && (Contains(context,"api/test")))
            {
                context.Response.StatusCode = 404;

                return;
            }

            await next.Invoke(context);
        }

        private static bool Contains(HttpContext context, string text)
        {
            return context.Request.Path.Value.ToLower().Contains(text);
        }
    }
}