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
            var nodiniteApiUrl = "https://demoenv1.nodinite.com/LogApi/api/";
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

            Serilog.Core.Logger nodiniteLogger = new LoggerConfiguration()
                .WriteTo.NodiniteApiSink(nodiniteApiUrl, settings)
                .CreateLogger();

            nodiniteLogger.Information("C# HTTP trigger function processed a request.");
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
