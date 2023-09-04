using Azure.Identity;
using Microsoft.Graph;

namespace MSGraph
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting");
            var scopes = new[] { "User.Read" };

            // Multi-tenant apps can use "common",
            // single-tenant apps must use the tenant ID from the Azure portal
            var tenantId = "efae2b71-7f88-46de-a2ef-372ed510ea49";
//var tenantId = "common";
            // Value from app registration
            var clientId = "0592c31c-3585-47f9-96f4-08e106bcb7bc";

            // using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // Callback function that receives the user prompt
            // Prompt contains the generated device code that you must
            // enter during the auth process in the browser
            Func<DeviceCodeInfo, CancellationToken, Task> callback = (code, _) => {
                Console.WriteLine($"Code: {code.Message}");
                return Task.FromResult(0);
            };

            Console.WriteLine("Getting code");
            // https://learn.microsoft.com/dotnet/api/azure.identity.devicecodecredential
            var deviceCodeCredential = new DeviceCodeCredential(
                callback, tenantId, clientId, options);

            Console.WriteLine("Getting client");
            var graphClient = new GraphServiceClient(deviceCodeCredential, scopes);

            // GET https://graph.microsoft.com/v1.0/me

            var user = await graphClient.Me
                //.Request()
                .GetAsync();

            Console.WriteLine($"User: {user.AboutMe}, {user.UserPrincipalName}");

            // GET https://graph.microsoft.com/v1.0/me/messages?$select=subject,sender&$filter=<some condition>&orderBy=receivedDateTime

            var messages = (await graphClient.Me.Messages
                    // .Request()
                    .GetAsync()
                    .ConfigureAwait(false))
                .Value
                .Select(m => new
                {
                    m.Subject,
                    m.Sender,
                    m.ReceivedDateTime
                })
                //.Filter("<filter condition>")
                .OrderByDescending(m => m.ReceivedDateTime)
                .ToList();

            Console.WriteLine($"Messages: {messages.Count}");

            foreach (var message in messages)
            {
                Console.WriteLine($"{message.Sender} -- {message.Subject} -- {message.ReceivedDateTime}");
            }
        }
    }
}