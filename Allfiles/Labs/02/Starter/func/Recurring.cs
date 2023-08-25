using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace func
{
    public class Recurring
    {
        [FunctionName("Recurring")]
        public void Run([TimerTrigger("0 0 0 0 * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Titus Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
