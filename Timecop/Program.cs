using Blazor.ClientStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Timecop;
using Timecop.Graph;
using Timecop.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddSingleton<ICheckInOutService, CheckInOutService>()
    .AddSingleton<IAnimationExtensions, AnimationExtensions>()
    .AddTransient<BlazorElement>()
    .AddScoped<ISyncService, SyncService>()
    .AddMudServices(config =>
    {
        config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
        config.SnackbarConfiguration.PreventDuplicates = false;
        config.SnackbarConfiguration.VisibleStateDuration = 3000;
        config.SnackbarConfiguration.HideTransitionDuration = 250;
        config.SnackbarConfiguration.ShowTransitionDuration = 250;
    });

builder.Services
    .AddBlazorClientStorage()
        .AddObjectStore<CheckInStateObjectStore>()
        .AddObjectStore<TimeActivityObjectStore>();

var baseUrl = builder.Configuration.GetSection("MicrosoftGraph")["BaseUrl"]!;


var scopes = builder.Configuration.GetSection("MicrosoftGraph:Scopes").Get<List<string>>();
builder.Services.AddGraphClient(baseUrl, scopes);

builder.Services.AddMsalAuthentication(options =>
{
    options.ProviderOptions.LoginMode = "redirect";
    options.ProviderOptions.DefaultAccessTokenScopes.Add("User.Read");
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
})
.AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, RemoteUserAccount, GraphUserAccountFactory>();

await builder.Build().RunAsync();
