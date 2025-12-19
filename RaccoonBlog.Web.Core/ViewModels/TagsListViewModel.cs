using RaccoonBlog.Web.Core.Infrastructure.Cammon;

namespace RaccoonBlog.Web.Core.ViewModels;

public class TagsListViewModel
{
    public string Name { get; set; }

    private string _slug;
    public string Slug
    {
        get { return _slug ??= SlugConverter.TitleToSlug(Name); }
    }

    public int Count { get; set; }
}