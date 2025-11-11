using Contracts.Auth;
using AuthService.Domain.Repositories;

namespace AuthService.Application.Commands;

public class PromoteToAdminHandler
{
    private readonly IAuthUserRepository _authUserRepository;

    public PromoteToAdminHandler(IAuthUserRepository authUserRepository)
    {
        _authUserRepository = authUserRepository;
    }

    public async Task<PromoteToAdminResponse> Handle(PromoteToAdminRequest request)
    {
        try
        {
            var user = await _authUserRepository.FindByIdAsync(request.UserId);

            if (user == null)
            {
                return new PromoteToAdminResponse(false, "User not found");
            }

            user.PromoteToAdmin();
            await _authUserRepository.UpdateAsync(user);

            Console.WriteLine($"✅ [AuthService] User {user.Username} promoted to Admin");

            return new PromoteToAdminResponse(true, $"User {user.Username} promoted to Admin successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthService] Error promoting user to admin: {ex.Message}");
            return new PromoteToAdminResponse(false, ex.Message);
        }
    }
}
