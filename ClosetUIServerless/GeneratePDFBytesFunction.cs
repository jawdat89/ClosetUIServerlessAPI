using ClosetUIServerless.Models;
using ClosetUIServerless.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ClosetUIServerless
{
    public class GeneratePDFBytesFunction
    {
        private readonly ILogger<GeneratePDFBytesFunction> _logger;
        private readonly IPDFService _pdfService;

        public GeneratePDFBytesFunction(ILogger<GeneratePDFBytesFunction> logger, IPDFService pdfService)
        {
            _logger = logger;
            _pdfService = pdfService;
        }

        [Function("GeneratePDFBytesFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            if (req.Method == "get")
            {
                _logger.LogInformation("C# HTTP trigger function processed a request.");
                return new OkObjectResult("Welcome to Azure Functions!");
            }
            var body = req.Body;
            if (body == null)
                return new BadRequestResult();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            dynamic model = JsonConvert.DeserializeObject(requestBody);

            if (model == null)
                return new BadRequestResult();

            var pdfBytes = await _pdfService.GenerateAndDownloadPdf(model);

            StreamDto streamDto = new()
            {
                Content = pdfBytes,
                ContentType = "application/pdf"
            };

            return new OkObjectResult(streamDto);
        }
    }
}
