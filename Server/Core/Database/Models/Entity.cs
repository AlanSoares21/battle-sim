namespace BattleSimulator.Server.Database.Models;

public class Entity
{
    public string Id { get; set; } = "";

    public int HealthRadius { get; set; }

    public int Damage { get; set; }

    public double DefenseAbsorption { get; set; }
}