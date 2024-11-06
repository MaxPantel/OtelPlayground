using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string serviceUrl = "http://prometheus:9090/api/v1/otlp/v1/metrics";
int exportIntervalMilliseconds = 1000;
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resourceBuilder =>
        resourceBuilder.AddService("My_Service")
            .AddAttributes(new List<KeyValuePair<string, object>>
            {
                new ("Location", "SA")
            })
        )
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder
            .AddAspNetCoreInstrumentation() 
            .AddHttpClientInstrumentation() 
            .AddRuntimeInstrumentation() 
            .AddProcessInstrumentation()
            .AddOtlpExporter((options, metricReaderOptions) =>
            {
                metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds =
                    exportIntervalMilliseconds;
            });
            // .AddOtlpExporter((options, metricReaderOptions) =>
            // {
            //     options.Endpoint = new Uri(serviceUrl);
            //     options.Protocol = OtlpExportProtocol.HttpProtobuf;
            //     metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds =
            //         exportIntervalMilliseconds;
            // });
    });


var app = builder.Build();
Console.WriteLine(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}