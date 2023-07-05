using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database.Models;

namespace BattleSimulator.Server.Hubs;

public interface IGameDbConverter
{
    IEntity Entity(Entity entity);
    IEntity DefaultEntity(string entityId);
}