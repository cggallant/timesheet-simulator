using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;

[Route("APIRequest")] // This allows for an '/' request (root level that returns a list of all available URIs supported by the URI). Unlikely to be needed but it's there :)
[Route("APIRequest/{level1}")] // This allows for an '/Employees/' request (1 deep)
[Route("APIRequest/{level1}/{level2}")] // This allows for an '/Employees/{EmployeeID}/' request (2 deep)
[Route("APIRequest/{level1}/{level2}/{level3}")] // This allows for an '/Assignments/Project/{ProjectID}' or '/Employees/Me/RegionalSettings' type request (3 deep)
[Route("APIRequest/{level1}/{level2}/{level3}/{level4}")] // Need 4 deep for things like '/ExpenseEntries/Sheet/{sSheetID}/Submit/', '/ExpenseEntries/Sheet/{sSheetID}/Approve/', '/ExpenseEntries/Sheet/{sSheetID}/Reject/'
[ApiController]
public class APIRequest: ControllerBase
{
    // Entry point for this class
    public async Task Index()
    {
        // To make it a bit easier to tell the different requests/responses appart
        HttpRequest CallerRequest = this.Request;
        HttpResponse ResponseToCaller = this.Response;

        // Grab the URI that was passed in (e.g. /APIRequest?version=7 or /APIRequest/Employees/?version=7)
        string FullUri = CallerRequest.Path;
        if (!FullUri.EndsWith("/")) { FullUri += "/"; } // In the event only APIRequest is called, make sure it ends in / so the next lines of code work
        FullUri += CallerRequest.QueryString.ToString();

        // Split the URI between the APIRequest/ part and the rest of the URI (e.g. /Employees/?version=7). We will need to
        // pass this request to the public API and that doesn't have 'APIRequest' in the endpoints.
        int EndPosition = (FullUri.IndexOf("APIRequest/", StringComparison.InvariantCultureIgnoreCase) + "APIRequest/".Length);
        string CallerRootUri = FullUri.Substring(0, EndPosition);
        string ApiPath = FullUri.Substring(EndPosition);

        // Determine the first part of the URI (e.g. http://localhost:49153/). If the request path didn't include this part, 
        // add it on (because we're going to be calling the main API, it will return https://api.dovico.com/ in the return data
        // which would break the module if it tried to use those URIs. We will need to swap that out with the root that we have
        // via the Timesheet endpoint)
        string TempRootUri = (CallerRequest.Scheme + "://" + CallerRequest.Host);
        if (!CallerRootUri.StartsWith(TempRootUri)) { CallerRootUri = (TempRootUri + CallerRootUri); }


        // Start building up the public API request
        string ApiRoot = "https://api.dovico.com/";
        HttpMethod Method = new HttpMethod(CallerRequest.Method); // GET, POST, PUT, or DELETE
        HttpRequestMessage ApiRequest = new HttpRequestMessage(Method, (ApiRoot + ApiPath));

        // Add in the credentials
        string ConsumerSecret = (string)HttpContext.Items["API_CONSUMER_SECRET"];
        string DataAccessToken = (string)HttpContext.Items["API_DATA_ACCESS_TOKEN"];
        ApiRequest.Headers.Authorization = new AuthenticationHeaderValue("WRAP", "access_token=\"client=" + ConsumerSecret + "&user_token=" + DataAccessToken + "\"");

        // Add in the Accept header if one was specified (if not specified, the API returns XML by default)
        ApiRequest.Headers.Accept.Clear();
        if (CallerRequest.Headers.ContainsKey("Accept"))
        {
            if (CallerRequest.Headers.Accept.Contains("application/json"))
            {
                ApiRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Timesheet is able to tell the local API what root URI to use when building up the response data but we don't have
                // that option when talking to the public endpoint. We'll need to do a replace on the resulting data so, given we're
                // dealing with Json, we need to adjust the forward slashes to match how they'll be returned.
                ApiRoot = ApiRoot.Replace("/", "\\/");
                CallerRootUri = CallerRootUri.Replace("/", "\\/");
            }
        }

        // If this is a request that can contain a body
        if ((Method == HttpMethod.Post) || (Method == HttpMethod.Put))
        {
            // Copy in the caller's body
            ApiRequest.Content = new StreamContent(CallerRequest.Body);

            // Set the content type if one was specified
            if (CallerRequest.ContentType != null)
            {
                ApiRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(CallerRequest.ContentType);
            }
        }


        // Fire off the request
        HttpClient client = new HttpClient();
        HttpResponseMessage ApiResponse = await client.SendAsync(ApiRequest);

        // Copy the response from the API to the caller's response
        ResponseToCaller.StatusCode = (int)ApiResponse.StatusCode;
        if (ApiResponse.Content.Headers.ContentType != null && ApiResponse.Content.Headers.ContentType.MediaType != null)
        {
            ResponseToCaller.ContentType = ApiResponse.Content.Headers.ContentType.MediaType;
        }

        // Get the body of the response as a string and replace all occurrances of https://api.dovico.com/ with the root that was used to 
        // make this request (e.g. http://localhost:49153/APIRequest/)
        string Body = await ApiResponse.Content.ReadAsStringAsync();
        byte[] BodyData = Encoding.UTF8.GetBytes(Body.Replace(ApiRoot, CallerRootUri, StringComparison.InvariantCultureIgnoreCase));
        await ResponseToCaller.Body.WriteAsync(BodyData, 0, BodyData.Length);
    }
}

