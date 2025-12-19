using RaccoonBlog.Web.Core;
using RaccoonBlog.Web.Core.Infrastructure.ActionFilters;
using RaccoonBlog.Web.Core.Infrastructure.Data;
using RaccoonBlog.Web.Core.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

var builder = WebApplication.CreateBuilder(args);

var settings = new Settings();
builder.Configuration.Bind(settings);

builder.Services.AddSingleton(settings);

builder.Services.AddSingleton<IDocumentStoreHolder>(provider =>
{
    var resolvedSettings = provider.GetRequiredService<Settings>();
    var logger = provider.GetRequiredService<ILogger<DocumentStoreHolder>>();
    return new DocumentStoreHolder(resolvedSettings.RavenSettings, logger);
});

builder.Services.AddSingleton<IDocumentStore>(sp => 
    sp.GetRequiredService<IDocumentStoreHolder>().DocumentStore);
builder.Services.AddScoped<IAsyncDocumentSession>(sp => 
    sp.GetRequiredService<IDocumentStoreHolder>().OpenAsyncSession());

builder.Services.AddScoped<RaccoonBlogContext>();

builder.Services.AddControllersWithViews(options =>
        {
            options.Filters.Add<BlogSetupFilter>();
        })
       .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
