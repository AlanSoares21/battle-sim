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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    public void Handle() 
    {
        
    }
}