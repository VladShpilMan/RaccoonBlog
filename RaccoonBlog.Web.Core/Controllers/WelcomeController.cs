using Microsoft.AspNetCore.Mvc;
using RaccoonBlog.Web.Core.Models;
using RaccoonBlog.Web.Core.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace RaccoonBlog.Web.Core.Controllers;

public class WelcomeController : Controller
{
    private readonly IAsyncDocumentSession _session;

    public WelcomeController(IAsyncDocumentSession session)
    {
        _session = session;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await AssertConfigurationIsNeeded();

        if (result != null) return result;

        return View(BlogConfig.New());
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateBlog(BlogConfig config)
    {
        var result = await AssertConfigurationIsNeeded();
        if (result != null)
            return result;

        if (!ModelState.IsValid)
            return View("Index", config);
        
        config.Id = BlogConfig.Key;
        await _session.StoreAsync(config);
        
        await _session.StoreAsync(new Section { Title = "Archive", IsActive = true, Position = 1, ControllerName = "Section", ActionName = "ArchivesList" });
        await _session.StoreAsync(new Section { Title = "Tags", IsActive = true, Position = 2, ControllerName = "Section", ActionName = "TagsList" });
        await _session.StoreAsync(new Section { Title = "Statistics", IsActive = true, Position = 3, ControllerName = "Section", ActionName = "PostsStatistics" });
        
        var user = new User
        {
            FullName = "Default User",
            Email = config.OwnerEmail,
            Enabled = true,
        }.SetPassword("raccoon");
        
        await _session.StoreAsync(user);
        
        await _session.SaveChangesAsync();

        return RedirectToAction("Success");
    }
    
    [HttpGet]
    public async Task<IActionResult> Success()
    {
        BlogConfig bc;
        
        using (_session.Advanced.DocumentStore.AggressivelyCacheFor(TimeSpan.Zero)) 
        {
            bc = await _session.LoadAsync<BlogConfig>(BlogConfig.Key);
        }

        return bc == null ? View("Index") : View(bc);
    }
    
    private async Task<IActionResult?> AssertConfigurationIsNeeded()
    {
        BlogConfig bc;

        using (await _session.Advanced.DocumentStore.AggressivelyCacheForAsync(TimeSpan.Zero))
        {
            bc = await _session.LoadAsync<BlogConfig>(BlogConfig.Key);
        }

        if (bc != null)
        {
            return RedirectToAction("List", "Posts"); 
        }
        return null;
    }
}