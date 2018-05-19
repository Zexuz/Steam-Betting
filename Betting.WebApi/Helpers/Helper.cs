namespace Betting.WebApi.Helpers
{
    public static class Helper
    {
        
//        public static void GetLocationForApiCall(HttpContext requestContext,Dictionary<string, object> dict, out string location)
//        {
//            // example route template: api/{controller}/{id}
//            var routeTemplate = requestContext.RouteData.Route.RouteTemplate;
//
//            var method = requestContext.Url.Request.Method; // GET, POST, etc.
//
//            foreach (var key in requestContext.RouteData.Values.Keys)
//            {
//                var value = requestContext.RouteData.Values[key].ToString();
//                if (Int64.TryParse(value, out long numeric)) // C# 7 inline declaration
//                    // must be numeric part of route
//                    dict.Add($"Route-{key}", value.ToString());
//                else                
//                    routeTemplate = routeTemplate.Replace("{" + key + "}", value);                
//            }
//
//            location = $"{method} {routeTemplate}";
//
//            var qs = HttpUtility.ParseQueryString(requestContext.Url.Request.RequestUri.Query);
//            var i = 0;
//            foreach (string key in qs.Keys)
//            {
//                var newKey = string.Format("q-{0}-{1}", i++, key);
//                if (!dict.ContainsKey(newKey))
//                    dict.Add(newKey, qs[key]);
//            }
//        }
    }
}