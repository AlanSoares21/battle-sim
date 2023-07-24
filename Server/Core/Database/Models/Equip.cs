using BattleSimulator.Engine;

namespace BattleSimulator.Server.Database.Models;

public class Equip 
{
    public List<Coordinate> Coordinates { get; set; } = new();
    public Engine.Equipment.EquipEffect Effect { get; set; }
    public EquipShape Shape { get; set; }
}

public enum EquipShape
{
    Rectangle
}