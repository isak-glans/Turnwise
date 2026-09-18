using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Turnwise.Application;
using Turnwise.Infrastructure;
using Turnwise.Web.Client;
using Turnwise.Web.Client.Services;
using Turnwise.Web.Client.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTurnwiseApplication();
builder.Services.AddTurnwiseInfrastructure();
builder.Services.AddScoped<EncounterSessionState>();
builder.Services.AddScoped<FileDownloadService>();

await builder.Build().RunAsync();
