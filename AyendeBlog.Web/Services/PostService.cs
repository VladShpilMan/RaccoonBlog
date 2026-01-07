using System.Text.Encodings.Web;
using AyendeBlog.Web.Infrastructure.Common;
using AyendeBlog.Web.Models;
using Microsoft.AspNetCore.Html;
using HtmlAgilityPack;

namespace AyendeBlog.Web.Services;

public class PostService
{
    private readonly BlogConfigurationService _configurationService;

    public PostService(BlogConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }
    
    public TimeToRead CalculateTimeToRead(IHtmlContent? body)
    {
        if (body == null) return TimeToRead.Empty;

        var writer = new StringWriter();
        body.WriteTo(writer, HtmlEncoder.Default);
        var bodyAsString = writer.ToString();

        var parts = bodyAsString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var wordCount = parts.Length;
        var rate = wordCount / 200.0;
        var minutes = (int)rate;
        var seconds = (rate - minutes) * 0.6;
        var minutesAndSeconds = minutes + seconds;
        var timeToReadInMinutes = (int)Math.Ceiling(minutesAndSeconds);
        
        return new TimeToRead
        {
            TimeToReadInMinutes = timeToReadInMinutes,
            WordCount = wordCount
        };
    }
    
    public string GetMetaDescription(IHtmlContent? body)
    {
        if (body == null) return string.Empty;

        const int maxLength = 160;
        const string ellipsis = "...";

        var writer = new StringWriter();
        body.WriteTo(writer, HtmlEncoder.Default);
        
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(writer.ToString());
        var bodyText = htmlDoc.DocumentNode.InnerText ?? string.Empty;

        if (bodyText.Length < maxLength)
            return bodyText;

        return bodyText.Substring(0, maxLength - ellipsis.Length) + ellipsis;
    }
    
    public string GetPostUrl(Post post)
    {
        var slug = SlugConverter.TitleToSlug(post.Title);
        var id = post.GetIdForUrl();
        var blogUrl = _configurationService.MainBlogUrl.TrimEnd('/');
        return $"{blogUrl}/{id}/{slug}";
    }
    
    public class TimeToRead
    {
        public static TimeToRead Empty = new TimeToRead { TimeToReadInMinutes = 0, WordCount = 0 };
        private int _timeToReadInMinutes;
        public int TimeToReadInMinutes
        {
            get => _timeToReadInMinutes;
            set => _timeToReadInMinutes = Math.Max(1, value);
        }
        public int WordCount { get; set; }
    }
}