using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Serilog.Core.Trackers;

namespace Betting.WebApi.Filters
{
    public class InterceptionAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string controllerName = "";
            string actionTemplet = "";
            string httpMethod = context.HttpContext.Request.Method;
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                controllerName = controllerActionDescriptor.ControllerName;
                var actionAttributes = controllerActionDescriptor.MethodInfo.GetCustomAttributes(true);
                foreach (var attribute in actionAttributes)
                {
                    switch (attribute)
                    {
                        case HttpMethodAttribute methodAttribute:
                            if (string.IsNullOrEmpty(methodAttribute.Template)) continue;
                            actionTemplet = methodAttribute.Template;
                            break;
                        case RouteAttribute routeAttribute:
                            if (string.IsNullOrEmpty(routeAttribute.Template)) continue;
                            actionTemplet = routeAttribute.Template;
                            break;
                    }
                }
            }

            context.HttpContext.Items["location"] = $"{httpMethod} : {controllerName}/{actionTemplet}";
            context.HttpContext.Items["arguments"] = context.ActionArguments as Dictionary<string, object>;


            var user = "Not logged in user";

            try
            {
                user = context.HttpContext.User.GetSteamId();
            }
            catch (Exception e)
            {
            }


            var dict = context.ActionArguments as Dictionary<string, object>;
            var logDetail = new PrefrencTracker("noName", user, $"{httpMethod} : {controllerName}/{actionTemplet}", "DomainName", "WebApi", dict);

            await next();
            
            //WE do not want to wait for a confirmation from the sink, We either did it successfull or not. DO NOT CARE!
            Task.Factory.StartNew(() => { logDetail.Stop();});
        }
    }
}