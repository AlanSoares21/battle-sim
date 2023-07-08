using BattleSimulator.Engine;

public class EntityEquip
{
    public string EntityId { get; set; } = "";
    public string EquipId { get; set; } = "";
    public List<Coordinate> Coordinates { get; set; } = new();
}