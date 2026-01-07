using AyendeBlog.Web.Models;
using Microsoft.AspNetCore.WebUtilities;
using Raven.Client.Documents.Session;
using RedditStatus = AyendeBlog.Web.Models.SocialNetwork.Reddit;

namespace AyendeBlog.Web.Services.Reddit;

public class RedditService
{
    private readonly IDocumentSession _documentSession;
    private readonly PostService _postService;
    
    public const string SendToRedditTag = "reddit";
    
    public RedditService(IDocumentSession documentSession, PostService postService)
    {
        _documentSession = documentSession;
        _postService = postService;
    }
    
    public string GetSubmitUrl(string subredditName, Post post)
    {
        var queryParams = new Dictionary<string, string?>
        {
            { "url", _postService.GetPostUrl(post) },
            { "title", post.Title },
            { "resubmit", "false" }
        };

        return QueryHelpers.AddQueryString($"https://www.reddit.com/r/{subredditName}/submit", queryParams);
    }
    
    public Credentials GetCredentials(BlogConfig blogConfig)
    {
        return new Credentials
        {
            User = blogConfig.RedditUser,
            Password = blogConfig.RedditPassword
        };
    }
    
    public IList<Post> GetPostsForAutomaticSubmission(DateTimeOffset currentDateTimeOffset)
    {
        return _documentSession.Query<Post>()
            .Where(x => x.PublishAt <= currentDateTimeOffset &&
                        x.TagsAsSlugs.Any(t => t == SendToRedditTag) &&
                        (x.Integration == null || 
                         x.Integration.Reddit == null ||
                         x.Integration.Reddit.Submitted == false))
            .OrderBy(x => x.PublishAt)
            .ToList();
    }
    
    public IList<Post> GetPostsForManualSubmission(DateTimeOffset currentDateTimeOffset)
    {
        return _documentSession.Query<Post>()
            .Where(x => x.PublishAt <= currentDateTimeOffset &&
                        x.TagsAsSlugs.Any(t => t == SendToRedditTag) &&
                        x.Integration != null &&
                        x.Integration.Reddit != null &&
                        (x.Integration.Reddit.PostSubmissions == null ||
                         x.Integration.Reddit.PostSubmissions.Any(
                             s => s.Status == RedditStatus.SubmissionStatus.CaptchaFailure || 
                                  s.Status == RedditStatus.SubmissionStatus.UnknownFailure)
                        ))
            
            .OrderBy(x => x.PublishAt)
            .ToList();
    }
    
    public IList<string> ParseSubreddits(BlogConfig config)
    {
        if (string.IsNullOrEmpty(config.RedditSubredditsToSubmitToOnPublish))
            return new List<string>();

        return config.RedditSubredditsToSubmitToOnPublish
            .Split(',')
            .Select(x => x.Trim())
            .ToList();
    }
}