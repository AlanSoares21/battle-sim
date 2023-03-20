using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Engine.Equipment;

namespace BattleSimulator.Server.Models;

public class Player : IEntity
{
    public Player(string identifier) {
        this.Id = identifier;
        Skills = new();
    }

    public Weapon Weapon { get; set; }

    public string Id { get; private set; }

    public List<ISkillBase> Skills { get; set; }
    public int HealthRadius { get; set; }
    public Coordinate CurrentHealth { get; set; }
    public int Damage { get; set; }
    public double DefenseAbsorption { get; set; }
}