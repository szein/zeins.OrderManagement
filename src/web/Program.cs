using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Radzen;
using WebApp;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    // options.ProviderOptions.DefaultAccessTokenScopes = builder.Configuration.GetSection("ApiSettings:Scopes").Get<IList<string>>() 
    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration.GetSection("ApiSettings:Scopes").Get<string[]>()[0]);
    options.ProviderOptions.LoginMode = "redirect";
});

builder.Services.AddHttpClient("Zeins_API", client => 
        client.BaseAddress = new Uri($"{builder.Configuration["ApiSettings:BaseUrl"]}/api/")
    ).AddHttpMessageHandler(sp => 
        sp.GetRequiredService<AuthorizationMessageHandler>()
          .ConfigureHandler(
              authorizedUrls: new[] { builder.Configuration["ApiSettings:BaseUrl"] },
              scopes: builder.Configuration.GetSection("ApiSettings:Scopes").Get<string[]>()
            )
        );

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Zeins_API"));
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IExceptionHandler, ExceptionHandler>();

builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();
