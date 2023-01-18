namespace BattleSimulator.Server.Models;

public class UserConnected
{
    public string name { get; set; } = "";
    public bool isOnBattle { get; set; } = false;
    public bool challengedByYou { get; set; } = false;
}