using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database.Models;

namespace BattleSimulator.Server.Hubs;

public interface IGameDb {
    Entity? SearchEntity(string entityId);
}