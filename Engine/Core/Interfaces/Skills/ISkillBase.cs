using System;
namespace BattleSimulator.Engine.Interfaces.Skills;

public interface ISkillBase {
    string Name { get; }
    int Points { get; }
    DateTime lastUse { get; }
    TimeSpan Cooldown { get; }
}