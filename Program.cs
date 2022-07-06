using System.Text;
using NBomber.Configuration;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;

var step = Step.Create("Fetch_transportOrderError_records", 
    clientFactory: HttpClientFactory.Create(),
    execute: async context =>
    {
        var json = await LoadJsonAsync();
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var request = Http.CreateRequest("POST", "https://localhost:7169/graphql")
            .WithHeader("Content-Type", "application/json")
            .WithBody(httpContent);

        return await Http.Send(request, context);
    });

var scenario = ScenarioBuilder
    .CreateScenario("Simple_GraphQL", step)
    .WithWarmUpDuration(TimeSpan.FromSeconds(5))
    .WithLoadSimulations(
        Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromSeconds(30)));

NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
    .Run();

async Task<string> LoadJsonAsync()
{
    var json = String.Empty;

    using var r = new StreamReader("TransportOrderErrors.json");
    {
        json = await r.ReadToEndAsync();
    }

    return json;
}