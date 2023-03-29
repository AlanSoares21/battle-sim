using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database.Models;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Database;

public class GameDb : IGameDb
{
    List<Entity> entities;
    public GameDb(IJsonSerializerWrapper serializer, IServerConfig serverConfig) 
    {
        string filePath;
        if (string.IsNullOrEmpty(serverConfig.DbFilePath))
            filePath = "gameDb.json";
        else
            filePath = serverConfig.DbFilePath;
        var content = serializer.DeserializeFile<List<Entity>>(filePath);
        if (content is null)
            entities = new();
        else
            entities = content;
    }

    public IEntity? SearchEntity(string entityId) 
    {
        var result = entities
            .Where(e => e.Id == entityId)
            .SingleOrDefault();
        if (result is null)
            return null;
        return PlayerFromEntity(result);
    }

    Player PlayerFromEntity(Entity entity) 
    {
        Player player = new(entity.Id);
        player.State.HealthRadius = entity.HealthRadius;
        player.DefensiveStats.DefenseAbsorption = entity.DefenseAbsorption;
        player.OffensiveStats.Damage = entity.Damage;
        return player;
    }
}