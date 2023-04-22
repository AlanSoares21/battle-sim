using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Engine.Skills;

namespace BattleSimulator.Server.Hubs;

public class SkillProvider : ISkillProvider
{
    Dictionary<string, ISkillBase> _Skills;
    public SkillProvider() 
    {
        _Skills = new();
        AddSkill(new BasicNegativeDamageOnX());
    }

    void AddSkill(ISkillBase skill) 
    {
        _Skills.Add(skill.Name, skill);
    }

    public bool Exists(string name) => _Skills.ContainsKey(name);

    public ISkillBase Get(string name) => _Skills[name];
}