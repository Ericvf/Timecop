using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Graph;
using System.Security.Claims;

namespace Timecop.Graph
{
    public class GraphUserAccountFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        private readonly ILogger<GraphUserAccountFactory> logger;
        private readonly GraphServiceClient graphServiceClient;

        public GraphUserAccountFactory(IAccessTokenProviderAccessor accessor, GraphServiceClient graphServiceClient, ILogger<GraphUserAccountFactory> logger)
            : base(accessor)
        {
            this.graphServiceClient = graphServiceClient;
            this.logger = logger;
        }

        public async override ValueTask<ClaimsPrincipal?> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
        {
            var initialUser = await base.CreateUserAsync(account, options);

            if (initialUser?.Identity?.IsAuthenticated ?? false)
            {
                try
                {
                    await AddCustomClaims(initialUser);
                }
                catch (AccessTokenNotAvailableException exception)
                {
                    logger.LogError($"Graph API access token failure: {exception.Message}");
                }
                catch (ServiceException exception)
                {
                    logger.LogError($"Graph API error: {exception.Message}");
                    logger.LogError($"Response body: {exception.RawResponseBody}");
                }
            }

            return initialUser;
        }

        private async Task AddCustomClaims(ClaimsPrincipal claimsPrincipal)
        {
            var user = await graphServiceClient.Me.GetAsync(config =>
            {
                config.QueryParameters.Select = new[] { "displayName", "mail" };
            });

            if (user != null)
            {
                logger.LogInformation($"Got user: {user.DisplayName} {user.Mail}");
                claimsPrincipal.AddUserGraphInfo(user);

                try
                {
                    var photo = await graphServiceClient.Me
                        .Photos["48x48"]  // Smallest standard size
                        .Content
                        .GetAsync();

                    claimsPrincipal.AddUserGraphPhoto(photo);
                }
                catch
                {
                    logger.LogError($"Error retrieving photo");
                }
            }
        }
    }
}
