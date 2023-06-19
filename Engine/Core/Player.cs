using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Engine.Equipment;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;
using System.Collections.Generic;

namespace BattleSimulator.Engine;

public class Player : IEntity
{
    public Player(string identifier) {
        this.Id = identifier;
        Skills = new();
        this.OffensiveStats = new OffensiveStats();
        this.DefensiveStats = new DefensiveStats();
        this.State = new State();
        Weapon = new Weapon() {
            damageOnX = DamageDirection.Positive,
            damageOnY = DamageDirection.Neutral
        };
    }

    public string Id { get; private set; }
    public Weapon Weapon { get; set; }
    public List<ISkillBase> Skills { get; set; }
    
    public IStateAttributes State { get; set; }
    public IOffensiveAttributes OffensiveStats { get; set; }
    public IDefensiveAttributes DefensiveStats { get; set; }
}

class OffensiveStats : IOffensiveAttributes
{
    public OffensiveStats() 
    {
        Damage = 10;
    }
    public int Damage { get; set; }
}

class DefensiveStats : IDefensiveAttributes
{
    public DefensiveStats() 
    {
        DefenseAbsorption = 0.1;
    }

    public double DefenseAbsorption { get; set; }
}

class State : IStateAttributes
{
    public State() 
    {
        HealthRadius = 25;
        CurrentHealth = new(0, 0);
    }

    public int HealthRadius { get; set; }
    public Coordinate CurrentHealth { get; set; }
}