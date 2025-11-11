using MassTransit;
using Contracts.Saga;
using Contracts.Users;
using Contracts.Auth;
using Contracts.Url;

namespace SagaService.Api.Consumers;

public class GetUserWithUrlsConsumer : IConsumer<GetUserWithUrlsRequest>
{
    private readonly IRequestClient<GetUserRequest> _getUserClient;
    private readonly IRequestClient<GetAuthByIdRequest> _getAuthClient;
    private readonly IRequestClient<GetUrlsByUserRequest> _getUrlsClient;

    public GetUserWithUrlsConsumer(
        IRequestClient<GetUserRequest> getUserClient,
        IRequestClient<GetAuthByIdRequest> getAuthClient,
        IRequestClient<GetUrlsByUserRequest> getUrlsClient)
    {
        _getUserClient = getUserClient;
        _getAuthClient = getAuthClient;
        _getUrlsClient = getUrlsClient;
    }

    public async Task Consume(ConsumeContext<GetUserWithUrlsRequest> context)
    {
        try
        {
            Console.WriteLine($"🚀 [Saga] Starting GetUserWithUrls for UserId: {context.Message.UserId}");

            // Step 1: Get user profile
            Console.WriteLine($"📤 [Saga] Requesting user profile...");
            var userResponse = await _getUserClient.GetResponse<GetUserResponse>(
                new GetUserRequest(context.Message.UserId),
                context.CancellationToken
            );
            var user = userResponse.Message;
            Console.WriteLine($"✅ [Saga] User retrieved: {user.Username}");

            // Step 2: Get auth info
            Console.WriteLine($"📤 [Saga] Requesting auth info...");
            string role = "User";
            bool isEmailVerified = false;

            try
            {
                var authResponse = await _getAuthClient.GetResponse<GetAuthByIdResponse>(
                    new GetAuthByIdRequest(user.AuthId),
                    context.CancellationToken,
                    RequestTimeout.After(s: 10)
                );
                role = authResponse.Message.Role;
                isEmailVerified = authResponse.Message.IsEmailVerified;
                Console.WriteLine($"✅ [Saga] Auth info retrieved for {user.Username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [Saga] Failed to get auth info: {ex.Message}. Using defaults.");
            }

            // Step 3: Get user's URLs
            Console.WriteLine($"📤 [Saga] Requesting user URLs...");
            List<UrlDto> urls = new();

            try
            {
                var urlsResponse = await _getUrlsClient.GetResponse<GetUrlsByUserResponse>(
                    new GetUrlsByUserRequest(context.Message.UserId),
                    context.CancellationToken,
                    RequestTimeout.After(s: 10)
                );
                urls = urlsResponse.Message.Urls;
                Console.WriteLine($"✅ [Saga] Retrieved {urls.Count} URLs for user {user.Username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [Saga] Failed to get URLs: {ex.Message}. Returning empty list.");
            }

            // Combine all data
            var result = new UserWithUrlsDto
            {
                UserId = user.Id,
                AuthId = user.AuthId,
                Username = user.Username,
                Email = user.Email,
                Role = role,
                IsEmailVerified = isEmailVerified,
                Urls = urls
            };

            Console.WriteLine($"✨ [Saga] Successfully aggregated user data for {user.Username}");
            await context.RespondAsync(new GetUserWithUrlsResponse(result));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Saga] Error in GetUserWithUrls: {ex.Message}");
            throw;
        }
    }
}
