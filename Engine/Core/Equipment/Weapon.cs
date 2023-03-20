namespace BattleSimulator.Engine.Equipment;

public class Weapon
{
    public string name { get; set; } = "unknown";
    public DamageDirection damageOnX { get; set; }
    public DamageDirection damageOnY { get; set; }
}
