using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BattleSimulator.Server.Workers;

public class AttacksHandlerWorker : BackgroundService
{
    IHubContext<GameHub, IGameHubClient> _hub;
    IAttacksRequestedList _attackList;
    IBattleCollection _battles;
    ILogger<AttacksHandlerWorker> _logger;
    public AttacksHandlerWorker(
        IHubContext<GameHub, IGameHubClient> hub,
        IAttacksRequestedList attackList,
        IBattleCollection battles,
        ILogger<AttacksHandlerWorker> logger) 
    {
        _hub = hub;
        _attackList = attackList;
        _battles = battles;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Attack handler woker start");
        const int delay = 500;
        while (!stoppingToken.IsCancellationRequested)
        {
            Handle();
            await Task.Delay(delay);
        }
        _logger.LogInformation("Attack handler woker stop");
    }

    public void Handle() 
    {
        var list = _attackList.ListAttacks();
        foreach (var attack in list)
        {
            string source = attack.Key;
            string target = attack.Value;
            var battle = _battles.Get(_battles.GetBattleIdByEntity(source));
            battle.Attack(target, source);
            if (!_attackList.RemoveAttack(source))
                FailOnRemoveAttack(source, target, battle.Id);
            _hub.Clients.Group(battle.Id.ToString())
                .Attack(
                    source, 
                    target, 
                    GetCurrentHealth(battle, target));
        }
    }

    void FailOnRemoveAttack(string source, string target, Guid battleid) 
    {
        _logger.LogError("Fail on remove attack from {source} on {target} - battle: {id}",
            source,
            target,
            battleid);
    }

    /*
        The method IBattle.Attack alredy calculate the current health value for 
        the target, thi value could be returned when the attack is executed,
        with that this method could be deleted
    */
    Coordinate GetCurrentHealth(IBattle battle, string id) 
    {
        var entity = battle.Entities.Where(e => e.Id ==id).Single();
        return entity.CurrentHealth;
    }
}