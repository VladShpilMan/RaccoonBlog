namespace AyendeBlog.Web.Services;

public class CommenterService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public const string CommenterCookieName = "commenter";

    public CommenterService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public void SetCommenterCookie(string commenterKey)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var options = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddYears(1),
            HttpOnly = true,
            IsEssential = true
        };

        context.Response.Cookies.Append(CommenterCookieName, commenterKey, options);
    }
}