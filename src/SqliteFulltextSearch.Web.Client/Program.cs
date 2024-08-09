// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using ElasticsearchFulltextExample.Shared.Client;
using ElasticsearchFulltextExample.Web.Client.Infrastructure;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<ApplicationErrorTranslator>();
builder.Services.AddScoped<ApplicationErrorMessageService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient<SearchClient>((services, client) =>
{
    client.BaseAddress = new Uri(builder.Configuration["SearchService:BaseAddress"]!);
});

builder.Services.AddLocalization();

// Fluent UI
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
