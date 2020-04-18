using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Umamimolecule.DurableFunctionExample
{
    public static class OrchestrationClient
    {
        public const string FunctionName = "OrchestrationClient";

        [FunctionName(FunctionName)]
        public static async Task<IActionResult> Run(
            [DurableClient]IDurableOrchestrationClient orchestrationClient,
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("OrchestrationClient called.");
            string instanceId = await orchestrationClient.StartNewAsync(Orchestrator.FunctionName);
            var payload = orchestrationClient.CreateHttpManagementPayload(instanceId);
            return new AcceptedResult(payload.StatusQueryGetUri, payload);
        }
    }
}
