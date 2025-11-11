using MassTransit;
using Contracts.Saga;
using Contracts.Users;
using Contracts.Auth;
using Contracts.Url;

namespace SagaService.Api.Consumers;

public class GetAllUsersWithUrlsConsumer : IConsumer<GetAllUsersWithUrlsRequest>
{
    private readonly IRequestClient<GetListUsersRequest> _getListUsersClient;
    private readonly IRequestClient<GetAuthsByIdsRequest> _getAuthsClient;
    private readonly IRequestClient<GetUrlsByUserIdsRequest> _getUrlsClient;

    public GetAllUsersWithUrlsConsumer(
        IRequestClient<GetListUsersRequest> getListUsersClient,
        IRequestClient<GetAuthsByIdsRequest> getAuthsClient,
        IRequestClient<GetUrlsByUserIdsRequest> getUrlsClient)
    {
        _getListUsersClient = getListUsersClient;
        _getAuthsClient = getAuthsClient;
        _getUrlsClient = getUrlsClient;
    }

    public async Task Consume(ConsumeContext<GetAllUsersWithUrlsRequest> context)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            Console.WriteLine($"🚀 [Saga] Starting OPTIMIZED GetAllUsersWithUrls at {startTime:HH:mm:ss.fff}");

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

            if (users.Count == 0)
            {
                await context.RespondAsync(new GetAllUsersWithUrlsResponse(new List<UserWithUrlsDto>()));
                return;
            }

            // Step 2: BATCH get all auth info in ONE request
            Console.WriteLine($"� [Saga] Batch requesting auth for {users.Count} users...");
            var authIds = users.Select(u => u.AuthId).ToList();
            var authsByIdDict = new Dictionary<Guid, (string Role, bool IsEmailVerified)>();
            
            try
            {
                var authsResponse = await _getAuthsClient.GetResponse<GetAuthsByIdsResponse>(
                    new GetAuthsByIdsRequest(authIds),
                    context.CancellationToken,
                    RequestTimeout.After(s: 10)
                );
                authsByIdDict = authsResponse.Message.AuthUsers.ToDictionary(
                    a => a.Id,
                    a => (a.Role, a.IsEmailVerified)
                );
                Console.WriteLine($"✅ [Saga] Retrieved {authsByIdDict.Count} auth records");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [Saga] Failed to batch get auths: {ex.Message}");
            }

            // Step 3: BATCH get all URLs in ONE request
            Console.WriteLine($"📤 [Saga] Batch requesting URLs for {users.Count} users...");
            var userIds = users.Select(u => u.Id).ToList();
            var urlsByUserIdDict = new Dictionary<Guid, List<UrlDto>>();
            
            try
            {
                var urlsResponse = await _getUrlsClient.GetResponse<GetUrlsByUserIdsResponse>(
                    new GetUrlsByUserIdsRequest(userIds),
                    context.CancellationToken,
                    RequestTimeout.After(s: 10)
                );
                urlsByUserIdDict = urlsResponse.Message.UrlsByUserId;
                var totalUrls = urlsByUserIdDict.Values.Sum(list => list.Count);
                Console.WriteLine($"✅ [Saga] Retrieved {totalUrls} URLs for {urlsByUserIdDict.Count} users");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [Saga] Failed to batch get URLs: {ex.Message}");
            }

            // Step 4: Build result by combining data
            var result = users.Select(user =>
            {
                var (role, isEmailVerified) = authsByIdDict.TryGetValue(user.AuthId, out var auth) 
                    ? auth 
                    : ("User", false);
                
                var urls = urlsByUserIdDict.TryGetValue(user.Id, out var userUrls) 
                    ? userUrls 
                    : new List<UrlDto>();

                return new UserWithUrlsDto
                {
                    UserId = user.Id,
                    AuthId = user.AuthId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = role,
                    IsEmailVerified = isEmailVerified,
                    Urls = urls
                };
            }).ToList();

            var endTime = DateTime.UtcNow;
            var totalTime = (endTime - startTime).TotalMilliseconds;
            Console.WriteLine($"✨ [Saga] Successfully retrieved all {result.Count} users with URLs in {totalTime}ms");
            Console.WriteLine($"⚡ Performance: {totalTime / result.Count:F2}ms per user (BATCH OPTIMIZED)");
            Console.WriteLine($"🎯 Queries: 3 total (1 users + 1 auths + 1 urls) instead of {1 + users.Count * 2}");
            
            await context.RespondAsync(new GetAllUsersWithUrlsResponse(result));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Saga] Error in GetAllUsersWithUrls: {ex.Message}");
            throw;
        }
    }
}
