// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchFulltextExample.Api.Infrastructure.Mvc
{
    public class EnumRouteConstraint<TEnum> : IRouteConstraint
        where TEnum : struct
    {
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var matchingValue = values[routeKey]?.ToString();

            return Enum.TryParse(matchingValue, true, out TEnum _);
        }
    }
}
