using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database.Models;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Engine;

namespace BattleSimulator.Server.Database;

public class GameDb : IGameDb
{
    ISkillProvider _skillProvider;
    List<Entity> _entities;
    ILogger<GameDb> _logger;
    public GameDb(
        IJsonSerializerWrapper serializer, 
        IServerConfig serverConfig,
        ISkillProvider skillProvider,
        ILogger<GameDb> logger) 
    {
        string filePath;
        _skillProvider = skillProvider;
        _logger = logger;
        if (string.IsNullOrEmpty(serverConfig.DbFilePath))
            filePath = "gameDb.json";
        else
            filePath = serverConfig.DbFilePath;
        var content = serializer.DeserializeFile<DbStructure>(filePath);
        if (content is null)
            _entities = new();
        else
        {
            _entities = content.Entities;
        }
    }

    public Entity? SearchEntity(string entityId) =>_entities
            .Where(e => e.Id == entityId)
            .SingleOrDefault();
}