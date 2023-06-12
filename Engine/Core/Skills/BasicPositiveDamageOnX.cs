using BattleSimulator.Engine.Equipment;

namespace BattleSimulator.Engine.Skills;

public class BasicPositiveDamageOnX : CommomDamageSkill
{
    public override string Name => "basicPositiveDamageOnX";

    protected override DamageDirection damageOnX => DamageDirection.Positive;

    protected override DamageDirection damageOnY => DamageDirection.Neutral;

}