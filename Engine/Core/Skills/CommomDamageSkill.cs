using BattleSimulator.Engine.Equipment;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.Skills;

namespace BattleSimulator.Engine.Skills;

public abstract class CommomDamageSkill : ISkillBase
{
    int _range = 5;
    int _skillDamage = 10;
    protected abstract DamageDirection damageOnX { get; }
    protected abstract DamageDirection damageOnY { get; }
    
    public abstract string Name { get; }

    public void Exec(IEntity target, IEntity source, IBattle battle)
    {
        var soucePosition = battle.Board.GetEntityPosition(source.Id);
        var targetPosition = battle.Board.GetEntityPosition(target.Id);
        if (soucePosition.Distance(targetPosition) > _range)
            return;
            
        battle.DealDamage(
            _skillDamage,
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