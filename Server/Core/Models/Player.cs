using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.Skills;

namespace BattleSimulator.Server.Models;

public class Player : IEntity
{
    public Player(string identifier) {
        this.Identifier = identifier;
        Skills = new();
    }

    public string Identifier { get; private set; }

    public List<ISkillBase> Skills { get; set; }

    public int Health { get; set; }
    public int HealthRegeneration { get; set; }
    public int Energy { get; set; }
    public int EnergyRegeneration { get; set; }
    public int PhysicalDamage { get; set; }
    public int MagicalDamage { get; set; }
    public double CriticalHit { get; set; }
    public double Accuracy { get; set; }
    public double AttackSpeed { get; set; }
    public double Penetration { get; set; }
    public double SkillsCooldown { get; set; }
    public double PhysicalDefenseAbsorption { get; set; }
    public double MagicalDefenseAbsorption { get; set; }
    public double Dodge { get; set; }
    public double Parry { get; set; }
    public double Block { get; set; }
    public double StealHealth { get; set; }
    public double DamageReflection { get; set; }
}