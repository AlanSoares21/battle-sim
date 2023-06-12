using BattleSimulator.Engine.Equipment;

namespace BattleSimulator.Engine.Skills;

public class BasicNegativeDamageOnY : CommomDamageSkill
{
    public override string Name => "basicNegativeDamageOnY";

    protected override DamageDirection damageOnX => DamageDirection.Neutral;

    protected override DamageDirection damageOnY => DamageDirection.Negative;

}