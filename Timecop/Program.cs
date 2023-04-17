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

#if DEBUG
builder.Logging.AddFilter(
    "Microsoft.AspNetCore.Components.WebAssembly.Authentication",
    LogLevel.Debug);
#endif

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

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://graph.microsoft.com") });

builder.Services.AddMsalAuthentication<RemoteAuthenticationState, RemoteUserAccount>(options =>
{
    options.ProviderOptions.Cache.StoreAuthStateInCookie = true;
    options.ProviderOptions.Cache.CacheLocation = "localStorage";
    options.ProviderOptions.LoginMode = "redirect";

    var scopes = builder.Configuration.GetValue<string>("GraphScopes");
    if (string.IsNullOrEmpty(scopes))
    {
        Console.WriteLine("WARNING: No permission scopes were found in the GraphScopes app setting. Using default User.Read.");
        scopes = "User.Read";
    }

    foreach (var scope in scopes.Split(';'))
    {
        Console.WriteLine($"Adding {scope} to requested permissions");
        options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
    }

    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
})
.AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, RemoteUserAccount, GraphUserAccountFactory>();

builder.Services.AddScoped<GraphClientFactory>();


await builder.Build().RunAsync();
