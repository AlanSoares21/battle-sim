using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Auth;

public interface IAuthService
{
    bool NameIsBeingUsed(string username);
    Task<(string accessToken, string refreshToken)> GenerateTokens(string username);
    Task<string> NewAccessToken(string username, string refreshToken);
}