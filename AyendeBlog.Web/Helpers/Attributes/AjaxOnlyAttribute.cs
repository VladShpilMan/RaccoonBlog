using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace AyendeBlog.Web.Helpers.Attributes;

public class AjaxOnlyAttribute : ActionMethodSelectorAttribute
{
    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
    {
        var request = routeContext.HttpContext.Request;
        
        return request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }
}