namespace BattleSimulator.Server.Models;

public class SuccessLoginResponse
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
}