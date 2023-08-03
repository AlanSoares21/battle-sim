using BattleSimulator.Engine;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public interface IGameHubClient
{
    Task ListConnectedUsers(IEnumerable<UserConnected> users);
    Task UserConnect(UserConnected newUser);
    Task UserDisconnect(UserConnected user);
    Task NewBattleRequest(BattleRequest battleRequest);
    Task BattleRequestSent(BattleRequest battleRequest);
    Task NewBattle(BattleData battleData);
    Task EntityMove(string entityIdentifier, double x, double y);
    Task BattleRequestCancelled(
        string cancellerId, 
        BattleRequest request);
    Task BattleCancelled(string cancellerId, Guid battleId);
    Task Attack(string source, string target, Coordinate currentHealth);
    Task Skill(
        string skillName, 
        string source, 
        string target, 
        Coordinate currentHealth);
    Task ManaRecovered();
}