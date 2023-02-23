public class ModuleInfoOptions
{
    public string Name { get; set; }
    public string MenuCaption { get; set; }
    public string StartPageUri { get; set; }

    public string ApiConsumerSecret { get; set; }
    public string ApiDataAcessToken { get; set; }
}

public class ModuleInfo
{
    private readonly RequestDelegate _next;
    private readonly ModuleInfoOptions _options;

    public ModuleInfo(RequestDelegate next, ModuleInfoOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (_options != null)
        {
            httpContext.Items["API_CONSUMER_SECRET"] = _options.ApiConsumerSecret;
            httpContext.Items["API_DATA_ACCESS_TOKEN"] = _options.ApiDataAcessToken;
            httpContext.Items["MODULE_NAME"] = _options.Name;
            httpContext.Items["MODULE_MENU_CAPTION"] = _options.MenuCaption;
            httpContext.Items["MODULE_STARTPAGE"] = _options.StartPageUri;
        }

        await _next.Invoke(httpContext);
    }
}