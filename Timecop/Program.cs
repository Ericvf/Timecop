using Blazor.ClientStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Timecop;
using Timecop.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddSingleton<ICheckInOutService, CheckInOutService>()
    .AddSingleton<IAnimationExtensions, AnimationExtensions>()
    .AddTransient<BlazorElement>()
    .AddMudServices(config =>
    {
        config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
        config.SnackbarConfiguration.PreventDuplicates = false;
        //config.SnackbarConfiguration.NewestOnTop = false;
        //config.SnackbarConfiguration.ShowCloseIcon = true;
        config.SnackbarConfiguration.VisibleStateDuration = 3000;
        config.SnackbarConfiguration.HideTransitionDuration = 250;
        config.SnackbarConfiguration.ShowTransitionDuration = 250;
        //config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
    });

builder.Services
    .AddBlazorClientStorage()
        .AddObjectStore<CheckInStateObjectStore>()
        .AddObjectStore<TimeActivityObjectStore>();

await builder.Build().RunAsync();
