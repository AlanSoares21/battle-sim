namespace BattleSimulator.Server.Database.Models;

public class Entity
{
    public string Id { get; set; } = "";

    public int HealthRadius { get; set; }

    public int Damage { get; set; }

    public double DefenseAbsorption { get; set; }

    public List<string> Skills { get; set; } = new();

    public List<Equip> Equips { get; set; } = new();
}
