using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Hubs;

public interface IBattleCollection
{
    List<IBattle> ListAll();
    public IBattle Get(Guid battleId);
    public Guid GetBattleIdByEntity(string entityIdentifier);
    public bool TryAdd(IBattle battle);
    public bool TryRemove(IBattle battle);
}