using MassTransit;
using Contracts.Saga;
using Contracts.Users;
using Contracts.Auth;
using Contracts.Url;

namespace SagaService.Api.Consumers;

public class GetAllUsersWithUrlsConsumer : IConsumer<GetAllUsersWithUrlsRequest>
{
    private readonly IRequestClient<GetListUsersRequest> _getListUsersClient;
    private readonly IRequestClient<GetAuthByIdRequest> _getAuthClient;
    private readonly IRequestClient<GetUrlsByUserRequest> _getUrlsClient;

    public GetAllUsersWithUrlsConsumer(
        IRequestClient<GetListUsersRequest> getListUsersClient,
        IRequestClient<GetAuthByIdRequest> getAuthClient,
        IRequestClient<GetUrlsByUserRequest> getUrlsClient)
    {
        _getListUsersClient = getListUsersClient;
        _getAuthClient = getAuthClient;
        _getUrlsClient = getUrlsClient;
    }

    public async Task Consume(ConsumeContext<GetAllUsersWithUrlsRequest> context)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            Console.WriteLine($"🚀 [Saga] Starting GetAllUsersWithUrls at {startTime:HH:mm:ss.fff}");

            // Step 1: Get all users from UserService
            Console.WriteLine($"📤 [Saga] Requesting all users...");
            var usersResponse = await _getListUsersClient.GetResponse<GetListUsersResponse>(
                new GetListUsersRequest(),
                context.CancellationToken,
                RequestTimeout.After(s: 30)
            );
            var users = usersResponse.Message.Users;
            var afterUsersTime = DateTime.UtcNow;
            Console.WriteLine($"✅ [Saga] Retrieved {users.Count} users in {(afterUsersTime - startTime).TotalMilliseconds}ms");

            var result = new List<UserWithUrlsDto>();

            // Step 2: For each user, get auth info and URLs
            foreach (var user in users)
            {
                Console.WriteLine($"🔄 [Saga] Processing user: {user.Username}");

                // Get auth info
                string role = "User";
                bool isEmailVerified = false;
                try
                {
                    var authResponse = await _getAuthClient.GetResponse<GetAuthByIdResponse>(
                        new GetAuthByIdRequest(user.AuthId),
                        context.CancellationToken,
                        RequestTimeout.After(s: 5)
                    );
                    role = authResponse.Message.Role;
                    isEmailVerified = authResponse.Message.IsEmailVerified;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [Saga] Failed to get auth for {user.Username}: {ex.Message}");
                }

                // Get URLs
                List<UrlDto> urls = new();
                try
                {
                    var urlsResponse = await _getUrlsClient.GetResponse<GetUrlsByUserResponse>(
                        new GetUrlsByUserRequest(user.Id),
                        context.CancellationToken,
                        RequestTimeout.After(s: 5)
                    );
                    urls = urlsResponse.Message.Urls;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [Saga] Failed to get URLs for {user.Username}: {ex.Message}");
                }

                result.Add(new UserWithUrlsDto
                {
                    UserId = user.Id,
                    AuthId = user.AuthId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = role,
                    IsEmailVerified = isEmailVerified,
                    Urls = urls
                });

                Console.WriteLine($"✅ [Saga] Processed {user.Username}: {urls.Count} URLs");
            }

            var endTime = DateTime.UtcNow;
            var totalTime = (endTime - startTime).TotalMilliseconds;
            Console.WriteLine($"✨ [Saga] Successfully retrieved all {result.Count} users with URLs in {totalTime}ms");
            Console.WriteLine($"⚡ Performance: {totalTime / result.Count:F2}ms per user");
            await context.RespondAsync(new GetAllUsersWithUrlsResponse(result));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Saga] Error in GetAllUsersWithUrls: {ex.Message}");
            throw;
        }
    }
}
