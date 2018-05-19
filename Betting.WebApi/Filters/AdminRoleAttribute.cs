using System.Threading.Tasks;
using Autofac;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Services.IoC;
using Betting.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Betting.WebApi.Filters
{
    public class AdminRoleAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var filters = context.Filters;
            foreach (var filterMetadata in filters)
            {
                if (filterMetadata.GetType() == typeof(AllowAnonymousFilter))
                {
                    await next();
                    return;
                }
            }

            var userSteamId = context.HttpContext.User.GetSteamId();
            var isAdmin     = await IoC.Container.Resolve<IStaffService>().IsAdmin(userSteamId);
            if (!isAdmin)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }

        public override async void OnActionExecuted(ActionExecutedContext context)
        {
            var filters = context.Filters;
            foreach (var filterMetadata in filters)
            {
                if (filterMetadata.GetType() == typeof(AllowAnonymousFilter))
                    return;
            }

            var userSteamId = context.HttpContext.User.GetSteamId();
            var isAdmin     = await IoC.Container.Resolve<IStaffService>().IsAdmin(userSteamId);
            if (!isAdmin)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }
}