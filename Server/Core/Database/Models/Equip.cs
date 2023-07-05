namespace BattleSimulator.Server.Database.Models;

public class Equip 
{
    public string Id { get; set; } = "";
    public Engine.Equipment.EquipEffect Effect { get; set; }
    public EquipShape Shape { get; set; }
    public List<Engine.Coordinate> Coordinates { get; set; } = new();
}

public enum EquipShape
{
    Rectangle
}