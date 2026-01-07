using AyendeBlog.Web.Infrastructure.Data;
using Raven.Client.Documents;

namespace AyendeBlog.Web.Infrastructure.Extensions;

public static class DbExtensions
{
    public static IServiceCollection AddRavenDb(this IServiceCollection services, IConfiguration configuration)
    {
        var ravenSettings = new NewDatabaseSettings();
        configuration.GetSection("RavenDb").Bind(ravenSettings);
        
        if (string.IsNullOrWhiteSpace(ravenSettings.DatabaseName))
            throw new InvalidOperationException("DB Name is missing!");
        
        services.AddSingleton(ravenSettings);
        
        services.AddSingleton<IDocumentStoreHolder, DocumentStoreHolder>();
        
        services.AddSingleton<IDocumentStore>(sp => 
            sp.GetRequiredService<IDocumentStoreHolder>().DocumentStore
        );
        
        services.AddScoped(sp => 
        {
            var store = sp.GetRequiredService<IDocumentStore>();
            return store.OpenSession();
        });
        
        services.AddScoped(sp => 
        {
            var store = sp.GetRequiredService<IDocumentStore>();
            return store.OpenAsyncSession();
        });

        return services;
    }
}