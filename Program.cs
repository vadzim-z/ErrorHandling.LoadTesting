using System.Text;
using NBomber.Configuration;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;

var transportOrderNumber = "AUTOFAnPbfk5lq";
var step = Step.Create("Fetch_transportOrderError_records", 
    clientFactory: HttpClientFactory.Create(),
    execute: async context =>
    {
        var json = await LoadJsonAsync("TransportOrderErrors.json");
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var request = Http.CreateRequest("POST", "https://localhost:7169/graphql")
            .WithHeader("Content-Type", "application/json")
            .WithBody(httpContent);

        return await Http.Send(request, context);
    });

var restApiStep = Step.Create("Fetch_transportOrder_by_Id-RestApi", 
    clientFactory: HttpClientFactory.Create(),
    execute: async context =>
    {
        var url = $"http://localhost:5000/api/transport-orders/{transportOrderNumber}/history";
        var request = Http.CreateRequest("GET", url)
            .WithHeader("Service-Key", "asdfasdflkjasdf")
            ;

        return await Http.Send(request, context);
    });
var graphQLStep = Step.Create("Fetch_transportOrder_by_Id-GraphQL", 
    clientFactory: HttpClientFactory.Create(),
    execute: async context =>
    {
        var json = await LoadJsonAsync("TransportOrderHistory.json");
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var request = Http.CreateRequest("POST", "http://localhost:5000/graphql")
            .WithHeader("Content-Type", "application/json")
            .WithBody(httpContent);

        return await Http.Send(request, context);
    });

var restApiScenario = ScenarioBuilder
    .CreateScenario("TransportOrder RestApi", restApiStep)
    .WithWarmUpDuration(TimeSpan.FromSeconds(5))
    .WithLoadSimulations(
        Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromSeconds(30)));

var graphQLScenario = ScenarioBuilder
    .CreateScenario("TransportOrder GraphQL", graphQLStep)
    .WithWarmUpDuration(TimeSpan.FromSeconds(5))
    .WithLoadSimulations(
        Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromSeconds(30)));

var errorHandlingScenario = ScenarioBuilder
    .CreateScenario("TransportOrderError GraphQL", graphQLStep)
    .WithWarmUpDuration(TimeSpan.FromSeconds(5))
    .WithLoadSimulations(
        Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromSeconds(30)));

NBomberRunner
    .RegisterScenarios(errorHandlingScenario/*, restApiScenario, graphQLScenario*/)
    .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
    .Run();

async Task<string> LoadJsonAsync(string path)
{
    var json = String.Empty;

    using var r = new StreamReader(path);
    {
        json = await r.ReadToEndAsync();
    }

    return json;
}