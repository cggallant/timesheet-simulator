using Microsoft.AspNetCore.StaticFiles;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// When deploying an app using IIS, each level can act as it's own website and IIS handles the mapping. With
// Kestrel everything from the app level down is controlled by the app. To allow for the Blazor non-standard
// static files (e.g. .dll), we need to add the mapping. Otherwise, you get a 404 error.
var extensionProvider = new FileExtensionContentTypeProvider();
extensionProvider.Mappings.Add(".dll", "application/octet-stream");
extensionProvider.Mappings.Add(".dat", "application/octet-stream"); // _framework/icudt_EFIGS.dat
extensionProvider.Mappings.Add(".blat", "application/octet-stream"); //_framework/dotnet.timezones.blat

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = extensionProvider
});

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

try 
{ 
    // Read in the manifest file's contents
    using (StreamReader sr = new StreamReader("wwwroot/modules/UserModule/ModuleManifest.json"))
    {
        string json = sr.ReadToEnd();
        ModuleInfoOptions? options = JsonSerializer.Deserialize<ModuleInfoOptions>(json);

        app.UseMiddleware<ModuleInfo>(options);
    }
} 
catch { } // Do nothing if the file doesn't exist (the dev will be redirected to the module not found view so they'll know about the issue)


// Need the following in order to get the APIRequest controller working
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
