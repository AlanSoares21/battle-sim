namespace BattleSimulator.Server.Database.Models;

public class DbStructure
{
    public List<Entity> Entities { get; set; } = new();
    public List<Equip> Equips { get; set; } = new();
    public List<EntityEquip> EntitiesEquips { get; set; } = new();
}