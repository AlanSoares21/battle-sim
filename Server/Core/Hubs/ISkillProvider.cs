using BattleSimulator.Engine.Interfaces.Skills;

namespace BattleSimulator.Server.Hubs;

public interface ISkillProvider
{
    bool Exists(string name);
    ISkillBase Get(string name);
}