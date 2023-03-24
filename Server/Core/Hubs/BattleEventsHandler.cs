namespace BattleSimulator.Server.Hubs;

public class BattleEventsHandler : IBattleEventsHandler
{
    IAttacksRequestedList _attacksList;
    ILogger<BattleEventsHandler> _logger;
    public BattleEventsHandler(
        IAttacksRequestedList attacksList,
        ILogger<BattleEventsHandler> logger) 
    {
        _attacksList = attacksList;
        _logger = logger;
    }
    public void Attack(string target, string callerId)
    {
        if (!_attacksList.RegisterAttack(callerId, target))
            _logger.LogError("Fail register attack from {caller} to {target}",
                callerId,
                target);
    }
}