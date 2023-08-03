using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BattleSimulator.Server.Workers;

public class ManaRecoveryWorker : BackgroundService
{
    ILogger<ManaRecoveryWorker> _logger;
    IHubContext<GameHub, IGameHubClient> _hub;
    IServerConfig _config;
    IBattleCollection _battles;
    public ManaRecoveryWorker(
        ILogger<ManaRecoveryWorker> logger, 
        IServerConfig config, 
        IHubContext<GameHub, IGameHubClient> hub,
        IBattleCollection battles
    )  {
        _logger = logger;
        _config = config;
        _hub = hub;
        _battles = battles;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int delay = _config.ManaRecoveryWorkerIntervalInMiliseconds;
        _logger.LogInformation("Mana recovery worker start - delay: {delay} (miliseconds)", delay);
        while (!stoppingToken.IsCancellationRequested)
        {
            await RecoverManaInAllBattles();
            await Task.Delay(delay);
        }
        _logger.LogInformation("Mana recovery worker stop");
    }

    async Task RecoverManaInAllBattles() {
        foreach (var battle in _battles.ListAll())
        {
            if ((DateTime.UtcNow - battle.ManaRecoveredAt).TotalSeconds >= 5)
                await battle.RecoverMana();
        }
    }
}