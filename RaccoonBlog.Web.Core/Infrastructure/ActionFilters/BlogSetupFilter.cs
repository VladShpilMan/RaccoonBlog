using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using RaccoonBlog.Web.Core.Models;
using RaccoonBlog.Web.Core.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace RaccoonBlog.Web.Core.Infrastructure.ActionFilters;

public class BlogSetupFilter : IAsyncActionFilter
{
    private readonly IAsyncDocumentSession _session;
    private readonly IDocumentStore _store;
    private readonly RaccoonBlogContext _context;

    public BlogSetupFilter(IAsyncDocumentSession session, IDocumentStore store, RaccoonBlogContext context)
    {
        _session = session;
        _store = store;
        _context = context;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        using (await _store.AggressivelyCacheForAsync(TimeSpan.FromMinutes(5)))
        {
            _context.Config = await _session.LoadAsync<BlogConfig>(BlogConfig.Key);
        }
        
        if (!_context.IsInstalled)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var controllerName = descriptor?.ControllerName;

            if (!string.Equals(controllerName, "Welcome", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Index", "Welcome", null);
                return;
            }
        }

        await next();
    }
}