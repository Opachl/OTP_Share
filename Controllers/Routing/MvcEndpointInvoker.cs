using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace OTP_Share.Controllers.Routing
{
  public class MvcEndpointInvoker
  {
    private readonly IActionInvokerFactory _invokerFactory;
    private readonly IActionContextAccessor _actionContextAccessor;

    public MvcEndpointInvoker(IActionInvokerFactory invokerFactory, IActionContextAccessor actionContextAccessor)
    {
      _invokerFactory = invokerFactory;
      _actionContextAccessor = actionContextAccessor;
    }

    public async Task InvokeAsync(HttpContext context, RouteValueDictionary routeValues)
    {
      var routeContext = new RouteContext(context);
      var actionContext = new ActionContext(context, new RouteData(routeValues), new ActionDescriptor());

      _actionContextAccessor.ActionContext = actionContext;
      var invoker = _invokerFactory.CreateInvoker(actionContext);
      await invoker.InvokeAsync();
    }
  }
}