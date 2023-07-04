using System.Collections.Generic;
using BattleSimulator.Engine.Equipment;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;
using BattleSimulator.Engine.Interfaces.Skills;

namespace BattleSimulator.Engine.Interfaces;

public interface IEntity
{
    string Id { get; }
    void ApplyDamage(Coordinate damage);
    void AddEquip(IEquip equip);
    List<ISkillBase> Skills { get; }
    Weapon Weapon { get; set; }
    IStateAttributes State { get; set; }
    IOffensiveAttributes OffensiveStats { get; set; } 
    IDefensiveAttributes DefensiveStats { get; set; } 
}