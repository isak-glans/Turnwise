using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Turnwise.Application;
using Turnwise.Infrastructure;
using Turnwise.Web.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Spike: proving the Domain/Application/Infrastructure layers - unchanged from the
// Blazor Server app - register and run correctly under a pure WebAssembly host.
builder.Services.AddTurnwiseApplication();
builder.Services.AddTurnwiseInfrastructure();

await builder.Build().RunAsync();
