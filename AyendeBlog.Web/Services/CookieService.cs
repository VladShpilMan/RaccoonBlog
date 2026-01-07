namespace AyendeBlog.Web.Services;

public class CookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public bool? HideSidebar => GetRequestCookieBool(CookieNames.HideSidebar);
    public int? VisitCount => GetRequestCookieInt(CookieNames.VisitCount);

    public CookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    private string? GetRequestCookieText(string name)
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[name];
    }

    private bool? GetRequestCookieBool(string name)
    {
        var value = GetRequestCookieText(name);
        
        if (bool.TryParse(value, out var result))
        {
            return result;
        }
        return null;
    }

    private int? GetRequestCookieInt(string name)
    {
        var value = GetRequestCookieText(name);
        
        if (int.TryParse(value, out var result))
        {
            return result;
        }
        return null;
    }
    
    private static class CookieNames
    {
        public const string HideSidebar = "hideSidebar";
        public const string VisitCount = "visitCount";
    }
}