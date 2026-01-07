namespace AyendeBlog.Web.Services;

public class SidebarService
{
    private const int VisitCountMax = 10;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CookieService _cookieService;

    public SidebarService(IHttpContextAccessor httpContextAccessor, CookieService cookieService)
    {
        _httpContextAccessor = httpContextAccessor;
        _cookieService = cookieService;
    }
    
    public bool ShouldShowSidebar()
    {
        if (_cookieService.HideSidebar == true)
            return false;
        
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null) return true;
        
        var isOnMainPage = request.Path.Value == "/";
        if (isOnMainPage)
            return true;
        
        var visitCount = _cookieService.VisitCount.GetValueOrDefault();
        return visitCount < VisitCountMax;
    }
}