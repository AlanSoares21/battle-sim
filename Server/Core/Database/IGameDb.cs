using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database.Models;

namespace BattleSimulator.Server.Database;

public interface IGameDb {
    void AddEntity(Entity entity);
    Entity? SearchEntity(string entityId);
    void UpdateEntity(Entity entity);
}