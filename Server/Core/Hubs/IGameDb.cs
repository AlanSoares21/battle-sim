using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Hubs;

public interface IGameDb {
    IEntity? SearchEntity(string entityId);
    IEntity GetEntityFor(string entityId);
}