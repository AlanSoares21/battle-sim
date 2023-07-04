namespace BattleSimulator.Engine.Equipment.Armor;

public class CommomBarrierEquip : IEquip
{
    public CommomBarrierEquip(IEquipFormat position)
    {
        Position = position;
    }
    public IEquipFormat Position { get; private set; }

    public EquipEffect Effect => EquipEffect.Barrier;
}