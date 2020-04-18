using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace Umamimolecule.DurableFunctionExample
{
    public class Activity
    {
        public const string FunctionName = "Activity";

        [FunctionName(FunctionName)]
        public async Task<string> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            var input = context.GetInput<string>();
            log.LogWarning($"{FunctionName} called for instance {context.InstanceId} with input {input}");
            await Task.Delay(5000);
            return Guid.NewGuid().ToString();
        }
    }
}
