using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Auth;

public interface IAuthService
{
    bool NameIsBeingUsed(string username);
    string CreateAccessToken(string username);
    string CreateRefreshToken();
    Task StoreRefreshToken(string username, string refreshToken);
    string GetUsernameFromAccessToken(string accessToken);
    Task<UserAuthenticated> GetUserAuthenticated(string username);
}