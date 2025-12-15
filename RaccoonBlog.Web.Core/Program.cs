using RaccoonBlog.Web.Core.Infrastructure.Data;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<NewDatabaseSettings>(builder.Configuration.GetSection("RavenSettings"));
builder.Services.AddSingleton<IDocumentStoreHolder, DocumentStoreHolder>();
builder.Services.AddSingleton<IDocumentStore>(sp => 
    sp.GetRequiredService<IDocumentStoreHolder>().DocumentStore);
builder.Services.AddScoped<IAsyncDocumentSession>(sp => 
    sp.GetRequiredService<IDocumentStoreHolder>().OpenAsyncSession());

// Add services to the container.
builder.Services.AddControllersWithViews()
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
