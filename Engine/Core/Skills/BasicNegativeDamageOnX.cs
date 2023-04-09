using BattleSimulator.Engine.Equipment;

namespace BattleSimulator.Engine.Skills;

public class BasicNegativeDamageOnX : CommomDamageSkill
{
    public override string Name => "basicNegativeDamageOnX";

    protected override DamageDirection damageOnX => DamageDirection.Negative;

    protected override DamageDirection damageOnY => DamageDirection.Neutral;

}