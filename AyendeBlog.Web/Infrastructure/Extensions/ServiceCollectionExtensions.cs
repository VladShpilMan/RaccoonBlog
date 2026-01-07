using AyendeBlog.Web.Services;
using AyendeBlog.Web.Services.Reddit;

namespace AyendeBlog.Web.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        
        services.AddSingleton<BlogConfigurationService>();
        services.AddScoped<CommenterService>();
        services.AddScoped<PostService>();
        services.AddScoped<CookieService>();
        services.AddScoped<RedditService>();
        
        services.AddHttpClient<Recaptcha2Service>(client =>
        {
            client.BaseAddress = new Uri("https://www.google.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        return services;
    }
}