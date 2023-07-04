namespace BattleSimulator.Engine.Equipment;

public interface IEquip {
    IEquipFormat Position { get; }
    EquipEffect Effect { get; }
}