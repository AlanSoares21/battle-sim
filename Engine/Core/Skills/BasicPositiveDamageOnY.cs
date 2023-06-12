using BattleSimulator.Engine.Equipment;

namespace BattleSimulator.Engine.Skills;

public class BasicPositiveDamageOnY : CommomDamageSkill
{
    public override string Name => "basicPositiveDamageOnY";

    protected override DamageDirection damageOnX => DamageDirection.Neutral;

    protected override DamageDirection damageOnY => DamageDirection.Positive;

}