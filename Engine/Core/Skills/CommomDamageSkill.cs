using BattleSimulator.Engine.Equipment;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.Skills;

namespace BattleSimulator.Engine.Skills;

public abstract class CommomDamageSkill : ISkillBase
{
    private int skillDamage = 10;
    protected abstract DamageDirection damageOnX { get; }
    protected abstract DamageDirection damageOnY { get; }
    
    public abstract string Name { get; }

    public void Exec(IEntity target, IEntity source, IBattle battle)
    {
        battle.DealDamage(
            skillDamage,
            source.OffensiveStats,
            target,
            this.damageOnX,
            this.damageOnY
        );
        battle.Notify.SkillDamage(
            this.Name, 
            source.Id, 
            target.Id, 
            target.State.CurrentHealth);
    }
}