namespace BattleSimulator.Engine.Equipment;

public interface IEquipFormat {
    bool IsInner(Coordinate coord);
    Coordinate? Intersect(Coordinate start, Coordinate end);
    Coordinate[] Coordinates { get; }
}