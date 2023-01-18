using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public interface IBattleRequestCollection
{
    IEnumerable<BattleRequest> List();
    BattleRequest Get(Guid requestId);
    bool TryAdd(BattleRequest newBattleRequest);
    bool TryRemove(BattleRequest request);

    IList<BattleRequest> RequestsWithUser(string userIdentifier);
}