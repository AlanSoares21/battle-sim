using System.Collections.Generic;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;
using BattleSimulator.Engine.Interfaces.Skills;

namespace BattleSimulator.Engine.Interfaces;

public interface IEntity: IStateAttributes, IOffensiveAttributes, IDefensiveAttributes 
{
    string Id { get; }
    List<ISkillBase> Skills { get; }
}