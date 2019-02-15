using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nodinite.Serilog.Models;
using Serilog;
using Nodinite.Serilog.ApiSink;

namespace MyFunctionProj
{
    public static class MyHttpTrigger
    {
        [FunctionName("MyHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            Microsoft.Extensions.Logging.ILogger log)
        {
            var nodiniteApiUrl = "https://{yourNodiniteInstance}/LogApi/api/";
            var settings = new NodiniteLogEventSettings()
            {
                LogAgentValueId = 503,
                EndPointDirection = 0,
                EndPointTypeId = 0,
                EndPointUri = "Nodinite.Serilog.ApiSink.AzureFunction.Serilog",
                EndPointName = "Nodinite.Serilog.ApiSink.AzureFunction",
                OriginalMessageTypeName = "Serilog.LogEvent",
                ProcessingUser = "NODINITE",
                ProcessName = "Azure.FunctionApp.MyFunctionProj.MyHttpTrigger",
                ProcessingMachineName = "Azure",
                ProcessingModuleName = "Azure.FunctionApp.MyFunctionProj",
                ProcessingModuleType = "Azure.FunctionApp"
            };

            string orderId = req.Query["orderId"];
            string correlationId = req.Query["correlationId"];
            
            Serilog.ILogger nodiniteLogger = new LoggerConfiguration()
                .WriteTo.NodiniteApiSink(nodiniteApiUrl, settings)
                .CreateLogger()
                .ForContext("CorrelationdId", correlationId)
                .ForContext("OrderId", orderId);

            string msg = $"Order #{orderId} processed.";
            nodiniteLogger.Information(msg);
            log.LogInformation(msg);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            return (ActionResult)new OkObjectResult(msg);
        }
    }
}
