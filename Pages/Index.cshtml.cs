using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TimesheetModuleSimulator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            bool ManifestFound = HttpContext.Items.ContainsKey("MODULE_STARTPAGE");
            string UserModuleMenuCaption = "Your Manifest File Was Not Found";
            string ModuleName = "ManifestNotFound";
            string StartPage = "index.html";            

            if (ManifestFound)
            {
                UserModuleMenuCaption = (string)HttpContext.Items["MODULE_MENU_CAPTION"];
                ModuleName = "UserModule"; // Default to the developer's module
            }

            // Even if the manifest wasn't found, there will be two views available that the developer can
            // switch between (the API Info and the Module not found view). The query string is only used
            // by the menu when switching views
            string ModuleNameQueryString = Request.Query["t"];
            if (ModuleNameQueryString != null)
            {
                // We want to override 'UserModule' with 'ManifestNotFound' if the manifest file wasn't
                // loaded. Otherwise, use the value from the query string.
                if (ManifestFound || ModuleNameQueryString != "UserModule")
                {
                    ModuleName = ModuleNameQueryString;
                }
            }

            // Grab the query string
            string? QueryString = Request.QueryString.Value;
            if (QueryString != null)
            {
                // Remove the "t=ModuleName" part. If only the '?' character remains then clear the string.
                QueryString = QueryString.Replace(("t=" + ModuleNameQueryString), "");
                if (QueryString == "?") { QueryString = ""; }
            }

            // If the developer's module is the view to show then grab the start page value that was set in the
            // manifest file.
            if (ModuleName == "UserModule") { StartPage = (string)HttpContext.Items["MODULE_STARTPAGE"]; }
           
            ViewData["UserModuleMenuCaption"] = UserModuleMenuCaption;
            ViewData["ModulePath"] = "modules/" + ModuleName + "/" + StartPage + QueryString;
            ViewData["ModuleDataApiRequestUri"] = string.Concat(this.Request.Scheme, "://", this.Request.Host, this.Request.Path, "APIRequest/");
        }
    }
}