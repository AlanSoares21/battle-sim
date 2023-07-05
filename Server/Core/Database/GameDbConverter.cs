using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Database.Models;
using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Database;

public class GameDbConverter : IGameDbConverter
{
    public static List<string> DefaultSkills = new() {
        "basicNegativeDamageOnX",
        "basicNegativeDamageOnY",
        "basicPositiveDamageOnX",
        "basicPositiveDamageOnY"
    };
    ISkillProvider _skillProvider;
    ILogger<GameDbConverter> _logger;
    public GameDbConverter(
        ISkillProvider provider,
        ILogger<GameDbConverter> logger)
    {
        _skillProvider = provider;
        _logger = logger;
    }
    public IEntity DefaultEntity(string entityId)
    {
        Player player = new(entityId);
        SetSkills(player, DefaultSkills);
        return player;
    }

    public IEntity Entity(Entity entity)
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