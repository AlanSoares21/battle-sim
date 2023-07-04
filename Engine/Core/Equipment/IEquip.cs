namespace BattleSimulator.Engine.Equipment;

public interface IEquip {
    IEquipFormat Format { get; }
    EquipEffect Effect { get; }
}