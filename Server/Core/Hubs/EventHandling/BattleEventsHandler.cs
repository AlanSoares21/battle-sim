namespace BattleSimulator.Server.Hubs.EventHandling;

public class BattleEventsHandler : IBattleEventsHandler
{
    IAttacksRequestedList _attacksList;
    ILogger<BattleEventsHandler> _logger;
    IBattleCollection _battles;
    IEventsQueue _eventsQueue;

    public BattleEventsHandler(
        IAttacksRequestedList attacksList,
        IBattleCollection battles,
        ILogger<BattleEventsHandler> logger,
        IEventsQueue eventsQueue) 
    {
        _attacksList = attacksList;
        _logger = logger;
        _battles = battles;
        _eventsQueue = eventsQueue;
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

    public void UseSkill(string target, string callerId, string skillName)
    {
        var skillEvent = new GameEvent(callerId, target);
        var caller = _battles
            .Get(_battles.GetBattleIdByEntity(callerId))
            .Entities.Find(e => e.Id == callerId);
        if (caller is null) {
            CallerNotFound(skillName, callerId);
            return;
        }
        var skill = caller.Skills.Find(s => s.Name == skillName);
        if (skill is null) {
            SkillNotFoundInCallerSkillSet(skillName, callerId);
            return;
        }
        skillEvent.SetSkill(skill);
       _eventsQueue.Enqueue(skillEvent); 
    }

    void CallerNotFound(string skill, string caller) {
        _logger.LogError("When try use skill {skill}, caller {caller} was not found.",
            skill,
            caller);
    }

    void SkillNotFoundInCallerSkillSet(string skill, string caller) {
        _logger.LogError("Skill {skill} not found in caller {caller} skill set",
            skill,
            caller);
    }
}