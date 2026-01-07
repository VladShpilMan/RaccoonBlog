using System.Net;
using System.Text.RegularExpressions;
using AyendeBlog.Web.Models;
using Microsoft.AspNetCore.Html;
using Markdig;

namespace AyendeBlog.Web.Helpers;

public static class DynamicContentHelpers
{
    private static readonly Regex CodeBlockFinder = new Regex(@"\[code lang=(.+?)\s*\](.*?)\[/code\]", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex FirstLineSpacesFinder = new Regex(@"^(\s|\t)+", RegexOptions.Compiled);
    
    public static HtmlString CompiledContent(this IDynamicContent? contentItem, bool trustContent)
    {
        if (contentItem == null) return new HtmlString(string.Empty);

        if (contentItem.ContentType == DynamicContentType.Html)
        {
            return trustContent ? new HtmlString(contentItem.Body) : new HtmlString(string.Empty);
        }
        
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseAutoIdentifiers()
            .Build();

        var contents = contentItem.Body;
        
        contents = CodeBlockFinder.Replace(contents, match => 
            GenerateCodeBlock(match.Groups[1].Value.Trim(), match.Groups[2].Value));
        try
        {
            var html = Markdown.ToHtml(contents, pipeline);
            
            return new HtmlString(html);
        }
        catch 
        {
            return new HtmlString($"<pre>{WebUtility.HtmlEncode(contents)}</pre>");
        }
    }
    
    private static string GenerateCodeBlock(string lang, string code)
    {
        code = WebUtility.HtmlDecode(code);
        
        return string.Format("<pre class=\"brush: {2}\">{0}{1}</pre>{0}", 
            Environment.NewLine,
            ConvertMarkdownCodeStatement(code).Replace("<", "&lt;"), 
            lang
        );
    }
    
    private static string ConvertMarkdownCodeStatement(string code)
    {
        var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var firstLineSpaces = GetFirstLineSpaces(lines.FirstOrDefault());
        var firstLineSpacesLength = firstLineSpaces.Length;
        
        var formattedLines = lines.Select(l => 
            l.Length < firstLineSpacesLength ? l : l.Substring(firstLineSpacesLength));
            
        return string.Join(Environment.NewLine, formattedLines);
    }

    private static string GetFirstLineSpaces(string? firstLine)
    {
        if (string.IsNullOrEmpty(firstLine)) return string.Empty;
        var match = FirstLineSpacesFinder.Match(firstLine);
        return match.Success ? firstLine.Substring(0, match.Length) : string.Empty;
    }
}