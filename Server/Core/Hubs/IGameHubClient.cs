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
    Task EntityMove(string entityIdentifier, int x, int y);
    Task BattleRequestCancelled(
        string cancellerId, 
        BattleRequest request);
    Task BattleCancelled(string cancellerId, Guid battleId);
}