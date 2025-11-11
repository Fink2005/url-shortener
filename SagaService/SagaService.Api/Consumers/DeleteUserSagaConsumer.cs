using MassTransit;
using Contracts.Saga;
using Contracts.Users;
using Contracts.Auth;

namespace SagaService.Api.Consumers;

public class DeleteUserSagaConsumer : IConsumer<DeleteUserSagaRequest>
{
    private readonly IRequestClient<DeleteUserRequest> _deleteUserClient;
    private readonly IRequestClient<DeleteAuthRequest> _deleteAuthClient;
    private readonly IRequestClient<GetUserRequest> _getUserClient;

    public DeleteUserSagaConsumer(
        IRequestClient<DeleteUserRequest> deleteUserClient,
        IRequestClient<DeleteAuthRequest> deleteAuthClient,
        IRequestClient<GetUserRequest> getUserClient)
    {
        _deleteUserClient = deleteUserClient;
        _deleteAuthClient = deleteAuthClient;
        _getUserClient = getUserClient;
    }

    public async Task Consume(ConsumeContext<DeleteUserSagaRequest> context)
    {
        try
        {
            Console.WriteLine($"🚀 [Saga] Starting DeleteUser for UserId: {context.Message.UserId}");

            // Step 1: Get user to find AuthId
            Console.WriteLine($"📤 [Saga] Getting user info...");
            var userResponse = await _getUserClient.GetResponse<GetUserResponse>(
                new GetUserRequest(context.Message.UserId),
                context.CancellationToken,
                RequestTimeout.After(s: 10)
            );
            var authId = userResponse.Message.AuthId;
            var username = userResponse.Message.Username;
            Console.WriteLine($"✅ [Saga] Found user: {username} with AuthId: {authId}");

            // Step 2: Delete from AuthService FIRST (safer - user can't login after this)
            Console.WriteLine($"📤 [Saga] Deleting auth from AuthService...");
            var deleteAuthResponse = await _deleteAuthClient.GetResponse<DeleteAuthResponse>(
                new DeleteAuthRequest(authId),
                context.CancellationToken,
                RequestTimeout.After(s: 10)
            );

            if (!deleteAuthResponse.Message.Success)
            {
                Console.WriteLine($"❌ [Saga] Failed to delete auth from AuthService");
                await context.RespondAsync(new DeleteUserSagaResponse(false, "Failed to delete authentication"));
                return;
            }
            Console.WriteLine($"✅ [Saga] Auth deleted from AuthService");

            // Step 3: Delete from UserService AFTER (if this fails, auth is already deleted - user can't login)
            Console.WriteLine($"📤 [Saga] Deleting user profile from UserService...");
            var deleteUserResponse = await _deleteUserClient.GetResponse<DeleteUserResponse>(
                new DeleteUserRequest(context.Message.UserId),
                context.CancellationToken,
                RequestTimeout.After(s: 10)
            );

            if (!deleteUserResponse.Message.Success)
            {
                Console.WriteLine($"⚠️ [Saga] Failed to delete user profile, but auth already deleted (user can't login)");
                await context.RespondAsync(new DeleteUserSagaResponse(false, "Auth deleted but failed to delete user profile. User cannot login."));
                return;
            }
            Console.WriteLine($"✅ [Saga] User profile deleted from UserService");

            Console.WriteLine($"✨ [Saga] Successfully deleted user: {username}");
            await context.RespondAsync(new DeleteUserSagaResponse(true, $"User {username} deleted successfully"));
        }
        catch (RequestFaultException faultEx)
        {
            var message = faultEx.Fault.Exceptions?.First()?.Message ?? "Unknown error";
            Console.WriteLine($"❌ [Saga] Request fault: {message}");
            await context.RespondAsync(new DeleteUserSagaResponse(false, message));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Saga] Error in DeleteUser: {ex.Message}");
            await context.RespondAsync(new DeleteUserSagaResponse(false, ex.Message));
        }
    }
}
