using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database.Models;

namespace BattleSimulator.Server.Database;

public interface IGameDb {
    Entity? SearchEntity(string entityId);
    List<Equip> GetEquips();
    void UpdateEntity(Entity entity);
}