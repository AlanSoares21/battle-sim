using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Workers;
/*
    Obs: O movement intention worker talvez não deveria existir,
    talvez o mais correto seria criar threads para um determinado número de
    batalhas e ai executar em loop as ações do worker. 
    Mais isso é puro achismo, eu nunca trabalhei com multithreading e por isso
    tenho várias dúvidas.
    Resolvi que por hora o melhor é implementar desse jeito já que o conceito
    de workers é mais familiar para mim (apesar de nunca ter criado um worker que
    executasse um loop em um espaço tão curto de tempo)  
*/

public class MovementIntentionsWorker : BackgroundService
{
    IBattleCollection _battles;
    ILogger<MovementIntentionsWorker> _logger;
    IServerConfig _config;
    public MovementIntentionsWorker(
        IBattleCollection battles, 
        ILogger<MovementIntentionsWorker> logger,
        IServerConfig config) 
    {
        _battles = battles;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Movement worker start");
        int delay = _config.MovementWorkerIntervalInMiliseconds;
        while (!stoppingToken.IsCancellationRequested)
        {
            await MoveEntities();
            await Task.Delay(delay);
        }
        _logger.LogInformation("Movement worker stop");
    }

    async Task MoveEntities() {
        foreach (var battle in _battles.ListAll())
            await battle.MoveEntities();
    }
}