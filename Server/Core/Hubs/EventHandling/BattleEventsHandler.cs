using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs.EventHandling;

public class BattleEventsHandler : IBattleEventsHandler
{
    IAttacksRequestedList _attacksList;
    ILogger<BattleEventsHandler> _logger;
    IBattleCollection _battles;

    public BattleEventsHandler(
        IAttacksRequestedList attacksList,
        IBattleCollection battles,
        ILogger<BattleEventsHandler> logger) 
    {
        _attacksList = attacksList;
        _logger = logger;
        _battles = battles;
    }
    public void Attack(string target, string caller)
    {
        if (
            CanAddAttack(caller, target) 
            && !_attacksList.RegisterAttack(caller, target))
                FailedOnRegisterAttack(caller, target);
    }

    bool CanAddAttack(string caller, string target) 
    {
        try {
            Guid battleId = _battles.GetBattleIdByEntity(caller);
            return _battles.Get(battleId).CanAttack(target, caller);
        }
        catch (KeyNotFoundException) {
            CallerTriedAttackSomeoneButIsentInABattle(caller, target);
            return false;
        }
    }

    void CallerTriedAttackSomeoneButIsentInABattle(string caller, string target) 
    {
        _logger.LogError("Caller {caller} tried attack {target} but isent in a battle",
            caller,
            target);
    }

    void FailedOnRegisterAttack(string caller, string target) 
    {
        _logger.LogError("Fail register attack from {caller} to {target}",
            caller,
            target);
    }

    public void Skill(string skillName, string target, CurrentCallerContext caller)
    {
        var battleId = _battles.GetBattleIdByEntity(caller.UserId);
        var battle = GetBattle(battleId);
        if (battle is null) 
            return;
        var callerEntity = battle.Entities.Single(e => e.Id == caller.UserId);
        var skill = callerEntity.Skills.Find(s => s.Name == skillName);
        if (skill is null) {
            _logger.LogError("In battle {id} skill {skill} was not found for user {user}",
                battleId,
                skillName,
                caller.UserId);
            return;
        }
        IEntity targetEntity;
        if (target == callerEntity.Id)
            targetEntity = callerEntity;
        else
            targetEntity = battle.Entities.Single(e => e.Id == target);
        skill.Exec(targetEntity, callerEntity, battle);
    }

    IBattle? GetBattle(Guid battleId) {
        try {
            return _battles.Get(battleId);
        }
        catch (KeyNotFoundException ex) {
            _logger.LogError("Battle {id} not found - Message: {message}",
                battleId,
                ex.Message);
            return null;
        }
    }
}