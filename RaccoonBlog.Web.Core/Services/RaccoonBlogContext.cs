using RaccoonBlog.Web.Core.Models;

namespace RaccoonBlog.Web.Core.Services;

public class RaccoonBlogContext
{
    public BlogConfig? Config { get; set; }
    
    public bool IsInstalled => Config != null;
    public int PageSize => Config?.PostsOnPage ?? 10;
}