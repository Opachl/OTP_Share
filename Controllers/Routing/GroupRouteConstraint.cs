namespace OTP_Share.Controllers.Constraints
{
  public class GroupRouteConstraint : IRouteConstraint
  {
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
      return httpContext?.Request.Query.ContainsKey("group") ?? false;
    }
  }
}