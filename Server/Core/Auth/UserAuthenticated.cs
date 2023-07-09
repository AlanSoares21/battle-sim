using Microsoft.AspNetCore.Identity;

namespace BattleSimulator.Server.Auth;

public class UserAuthenticated
{
    public string Id { get; set; } = "";
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

}