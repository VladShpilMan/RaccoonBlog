using Microsoft.AspNetCore.Mvc;
using RaccoonBlog.Web.Core.Infrastructure.Indexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace RaccoonBlog.Web.Core.ViewComponents.Sections;

public class ArchivesListViewComponent : ViewComponent
{
    private readonly IAsyncDocumentSession _session;

    public ArchivesListViewComponent(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var now = DateTime.Now;

        using (_session.Advanced.DocumentStore.AggressivelyCacheFor(TimeSpan.FromHours(1)))
        {
            var dates = await _session.Query<Posts_ByMonthPublished_Count.ReduceResult, Posts_ByMonthPublished_Count>()
                                      .OrderByDescending(x => x.Year)
                                      .ThenByDescending(x => x.Month)
                                      .Take(1024)
                                      .Where(x => x.Year < now.Year || x.Year == now.Year && x.Month <= now.Month)
                                      .ToListAsync();
            
            return View(dates);
        }
    }
}