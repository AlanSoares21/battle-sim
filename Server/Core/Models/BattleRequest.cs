namespace BattleSimulator.Server.Models;

public class BattleRequest
{
    public string requester { get; set; } = "";
    public Guid requestId { get; set; }
    public string target { get; set; } = "";
    public bool UserIsOnRequest(string userIdentifier) => 
        requester == userIdentifier || target == userIdentifier;
}