using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Authentication.WebAssembly.Msal.Models;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Timecop.Graph
{
    internal static class GraphClientExtensions
    {
        public static IServiceCollection AddGraphClient(this IServiceCollection services, string? baseUrl, List<string>? scopes)
        {
            if (string.IsNullOrEmpty(baseUrl) || scopes?.Count == 0)
            {
                return services;
            }

            services.Configure<RemoteAuthenticationOptions<MsalProviderOptions>>(
                options =>
                {
                    scopes?.ForEach((scope) =>
                    {
                        options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
                    });
                });

            services.AddScoped<IAuthenticationProvider, GraphAuthenticationProvider>();

            services.AddScoped(sp =>
            {
                return new GraphServiceClient(
                    new HttpClient(),
                    sp.GetRequiredService<IAuthenticationProvider>(),
                    baseUrl);
            });

            return services;
        }

        private class GraphAuthenticationProvider(IAccessTokenProviderAccessor accessor, IConfiguration config) : IAuthenticationProvider
        {
            private readonly IAccessTokenProviderAccessor accessor = accessor;
            private readonly IConfiguration config = config;

            public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
            {
                var result = await accessor.TokenProvider.RequestAccessToken(
                    new AccessTokenRequestOptions()
                    {
                        Scopes = config.GetSection("MicrosoftGraph:Scopes").Get<string[]>()
                    });

                if (result.TryGetToken(out var token))
                {
                    request.Headers.Add("Authorization", $"{CoreConstants.Headers.Bearer} {token.Value}");
                }
            }
        }
    }
}
