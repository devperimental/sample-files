using ExternalApi.Filters;
using System.Diagnostics.CodeAnalysis;
using TestServiceLayer;
using TestServiceLayer.Shared.Behaviours;
using Helper.Startup.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load bootstrapConfiguration
var bootstrapConfiguration = builder.Services.AddBootstrapConfiguration();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

// Configure the container dependancies
builder.Services.AddWebApiService(builder.Configuration, builder.Logging, bootstrapConfiguration);

builder.Services.AddScoped<HandleExceptionAttribute>();
builder.Services.AddTransient<ITestCallback, TestCallback>();

// Add services to the container.
builder.Services.AddControllers(
    options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// DI Entry point
/// </summary>
[ExcludeFromDescription]
[ExcludeFromCodeCoverage]
public partial class Program { }