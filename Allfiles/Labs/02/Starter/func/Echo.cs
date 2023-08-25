using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

/*The following lines of code to add using directives for the 
Microsoft.AspNetCore.Mvc, Microsoft.Azure.WebJobs, 
Microsoft.AspNetCore.Http, and Microsoft.Extensions.Logging namespaces.*/
namespace func;

public static class Echo
{/*The following code block to create a new public static method
named Run that returns a variable of type IActionResult and that
also takes in variables of type HttpRequest and ILogger as parameters
named request and logger.*/
    [FunctionName("Echo")]
    public static async Task<IActionResult> Run(
        [HttpTrigger("POST")] HttpRequest request,
        ILogger logger)
    {
        logger.LogInformation("Hi Titus, I received a request");
        var sr = new StreamReader(request.Body);
        var bodyAsTxt = await sr.ReadToEndAsync().ConfigureAwait(false);
        return new OkObjectResult($"I repeat you: '{bodyAsTxt}'");
        /*The following line of code to echo the body of the HTTP request as the HTTP response.*/
    }
}