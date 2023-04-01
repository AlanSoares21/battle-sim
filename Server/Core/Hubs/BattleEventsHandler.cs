namespace BattleSimulator.Server.Hubs;

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
}