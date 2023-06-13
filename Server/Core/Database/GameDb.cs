using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database.Models;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Engine;

namespace BattleSimulator.Server.Database;

public class GameDb : IGameDb
{
    public static List<string> DefaultSkills = new() {
        "basicNegativeDamageOnX",
        "basicNegativeDamageOnY",
        "basicPositiveDamageOnX",
        "basicPositiveDamageOnY"
    };
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
        var content = serializer.DeserializeFile<List<Entity>>(filePath);
        if (content is null)
            _entities = new();
        else
            _entities = content;
    }

    public IEntity GetEntityFor(string entityId)
    {
        var entity = SearchEntity(entityId);
        if (entity is null)
            return DefaultPlayer(entityId);
        return entity;
    }

    Player DefaultPlayer(string id)
    {
        Player player = new(id);
        SetSkills(player, DefaultSkills);
        return player;
    }

    public IEntity? SearchEntity(string entityId) 
    {
        var result = _entities
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
        SetSkills(player, entity.Skills);
        return player;
    }

    void SetSkills(Player player, List<string> skills) 
    {
        foreach (var skill in skills)
        {
            if (_skillProvider.Exists(skill))
                player.Skills.Add(_skillProvider.Get(skill));
            else
                _logger.LogWarning("Skill {skill} not found. Player: {player}", skill, player.Id);
        }
    }
}