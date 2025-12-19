using Microsoft.AspNetCore.Mvc;
using RaccoonBlog.Web.Core.Infrastructure.Indexes;
using RaccoonBlog.Web.Core.Services;
using RaccoonBlog.Web.Core.ViewModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace RaccoonBlog.Web.Core.ViewComponents.Sections;

public class TagsListViewComponent : ViewComponent
{
    private readonly IAsyncDocumentSession _session;
    private readonly RaccoonBlogContext _context;

    public TagsListViewComponent(IAsyncDocumentSession session, RaccoonBlogContext context)
    {
        _session = session;
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var mostRecentTag = new DateTimeOffset(DateTimeOffset.UtcNow.Year - 2, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, DateTimeOffset.UtcNow.Offset);
        var minPosts = _context.Config?.MinNumberOfPostForSignificantTag ?? 1;

        using (_session.Advanced.DocumentStore.AggressivelyCacheFor(TimeSpan.FromHours(1)))
        {
            var tags = await _session.Query<Tags_Count.ReduceResult, Tags_Count>()
                                     .Where(x => x.Count > minPosts && x.LastSeenAt > mostRecentTag)
                                     .OrderBy(x => x.Name)
                                     .ToListAsync();

            // МАППИНГ (Как в старом проекте)
            var vm = tags.Select(t => new TagsListViewModel
            {
                Name = t.Name,
                Count = t.Count
            }).ToList();

            return View(vm);
        }
    }
}