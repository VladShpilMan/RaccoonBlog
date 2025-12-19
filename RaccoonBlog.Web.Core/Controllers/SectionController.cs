using Microsoft.AspNetCore.Mvc;

namespace RaccoonBlog.Web.Core.Controllers;

public class SectionController : Controller
{
    public IActionResult TagsList()
    {
        return View();
    }
    
    public IActionResult ArchivesList()
    {
        return View();
    }
    
    public IActionResult PostsStatistics()
    {
        return View();
    }
}