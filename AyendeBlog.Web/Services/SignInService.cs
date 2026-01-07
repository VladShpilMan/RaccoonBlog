using System.Security.Claims;
using AyendeBlog.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AyendeBlog.Web.Services;

public class SignInService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SignInService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task SignInAsync(LogOnModel logOn, bool isPersistent)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;
        
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, logOn.Login),
            new Claim(ClaimTypes.Email, logOn.Login),
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent || logOn.RememberMe,
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        };
        
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
    
    public async Task SignOutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;
        
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}