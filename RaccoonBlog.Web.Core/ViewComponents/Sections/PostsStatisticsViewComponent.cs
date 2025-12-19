using Microsoft.AspNetCore.Mvc;
using RaccoonBlog.Web.Core.Infrastructure.Indexes;
using RaccoonBlog.Web.Core.ViewModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace RaccoonBlog.Web.Core.ViewComponents.Sections;

public class PostsStatisticsViewComponent : ViewComponent
{
    private readonly IAsyncDocumentSession _session;

    public PostsStatisticsViewComponent(IAsyncDocumentSession session)
    {
        _session = session;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        using (await _session.Advanced.DocumentStore.AggressivelyCacheForAsync(TimeSpan.FromMinutes(6)))
        {
            var statistics = await _session.Query<Posts_Statistics.ReduceResult, Posts_Statistics>()
                                           .FirstOrDefaultAsync() ?? new Posts_Statistics.ReduceResult();
            
            var vm = new PostsStatisticsViewModel
            {
                PostsCount = statistics.PostsCount,
                CommentsCount = statistics.CommentsCount
            };

            return View(vm);
        }
    }
}