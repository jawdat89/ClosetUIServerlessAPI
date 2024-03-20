using ClosetUIServerless.Services;
using ClosetUIServerless.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        //services.AddApplicationInsightsTelemetryWorkerService();
        //services.ConfigureFunctionsApplicationInsights();
        services.AddScoped<IPDFService, PDFService>();
    })
    .Build();

QuestPDF.Settings.License = LicenseType.Community;

host.Run();
