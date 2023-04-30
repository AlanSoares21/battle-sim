using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public interface IBattleHandler
{
    void CreateDuel(BattleRequest request, CurrentCallerContext caller);
}