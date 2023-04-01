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
    IHubContext<GameHub, IGameHubClient> hub;
    IGameHubState state;
    ILogger<MovementIntentionsWorker> _logger;
    public MovementIntentionsWorker(IHubContext<GameHub, IGameHubClient> hubContext, IGameHubState hubState, ILogger<MovementIntentionsWorker> logger) {
        hub = hubContext;
        state = hubState;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Movement worker start");
        const int delay = 500;
        while (!stoppingToken.IsCancellationRequested)
        {
            MoveEntities();
            await Task.Delay(delay);
        }
        _logger.LogInformation("Movement worker stop");
    }

    public void MoveEntities() {
        List<MovementIntention> intentionsToRemove = new();
        foreach (var intention in state.MovementIntentions.List()) {
            bool canRemove = false;
            HandleMove(intention, out canRemove);
            if (canRemove)
                intentionsToRemove.Add(intention);
        }
        foreach (var intention in intentionsToRemove)
            if (!state.MovementIntentions.TryRemove(intention))
                LogFailToRemoveIntention(intention);
    }

    /* OBS:: essa função tem o parametro de saida canRemove  por que
        não consegui pensar em um bom nome para uma função que 
        executa o movimento e retorna se pode ou não deletar a intention
    */
    void HandleMove(MovementIntention intention, out bool canRemove) {
        IBattle? battle = TryGetBattleWithEntity(intention.entityIdentifier);
        if (battle is null) {
            canRemove = true;
            return;
        }
        try {
            MoveEntity(intention, battle);
        }
        catch (EntityNotMovedException) {
            canRemove = true;
            return;
        }
        Coordinate entityCellAfterMove = battle.Board
            .GetEntityPosition(intention.entityIdentifier);
        hub.Clients
            .Group(battle.Id.ToString())
            .EntityMove(
                intention.entityIdentifier,
                entityCellAfterMove.X,
                entityCellAfterMove.Y);
        canRemove = entityCellAfterMove.IsEqual(intention.cell);
    }

    IBattle? TryGetBattleWithEntity(string entityId) {
        try {
            Guid battleId = state.Battles.GetBattleIdByEntity(entityId);
            return state.Battles.Get(battleId);
        }
        catch (KeyNotFoundException ex) {
            LogBattleWithEntityNotFound(entityId, ex);
            return null;
        }
    }

    void MoveEntity(MovementIntention intention, IBattle battle) 
    {
        IEntity entity = battle.Entities
            .Where(e => e.Id == intention.entityIdentifier)
            .Single();
        TryMove(intention.cell, entity, battle);
    }

    void LogBattleWithEntityNotFound(string entityId, KeyNotFoundException ex) {
        _logger.LogError("Entity {id} is not in a battle. Message: {message}", 
            entityId,
            ex.Message);
    }

    void TryMove(Coordinate targetCell, IEntity entity, IBattle battle) {
        Coordinate sourceCell = battle.Board.GetEntityPosition(entity.Id);
        MoveDirection direction = GetDirectionToCell(sourceCell, targetCell);
        try {
            battle.Move(entity, direction);
        } 
        catch (Exception ex) {
            LogErrorOnMoveEntity(entity.Id, direction, ex);
            throw new EntityNotMovedException();
        }
    }

    /*
        estou (0,0)
        quero (3,3)
        
        se estou == quero
            erro -> atual não deve ser igual a quero

        Prioridade para x, depois  y
        Se diferenca entr x e maior ou igual que a diferença de y
            se x(atual) menor que x(quero)
                aumenta x
            se x(atual) maior que x(quer)
                deminui x
        se y(atual) menor que y(quero)
            aumenta y
        se y(atual) maior que y(quero)
            diminui y
    */
    private MoveDirection GetDirectionToCell(Coordinate source, Coordinate destination) {
        int sourceX = (int)source.X;
        int sourceY = (int)source.Y;

        int destinationX = (int)destination.X;
        int destinationY = (int)destination.Y;
        
        int diffX = Math.Abs(sourceX - destinationX);
        int diffY = Math.Abs(sourceY - destinationY);

        if (diffX >= diffY)
            if (destination.X > source.X)
                return MoveDirection.Right;
            else
                return MoveDirection.Left;
        if (destination.Y > source.Y)
            return MoveDirection.Up;
        return MoveDirection.Down;
    }

    void LogErrorOnMoveEntity(string entityId, MoveDirection direction, Exception ex) {
        _logger.LogError(ex, "Error on move entity {id} to direction {direction}.", 
            entityId, 
            direction);
    }

    void LogFailToRemoveIntention(MovementIntention intention) {
        _logger.LogError("Fail on remove movement intention - entity: {entity} - cell: {cell}",
            intention.entityIdentifier, 
            intention.cell);
    }
}

public class EntityNotMovedException: Exception {}