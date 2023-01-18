using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Auth;

public interface IAuthService
{
    bool NameIsBeingUsed(string username);
    string GenerateJwtToken(string username);
}