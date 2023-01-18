using System;
using System.Collections.Generic;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Engine;

public class Duel : IBattle
{
    public Duel(Guid battleId, IBoard board) {
        this.Board = board;
        this.Entities = new();
        Id = battleId;
    }
    public Guid Id { get; private set; }
    public List<IEntity> Entities { get; private set; }
    public IBoard Board { get; private set; }

    public void AddEntity(IEntity entity)
    {
        if (this.Entities.Count == 2)
            return;
        AddEntity(entity, FindCellToAddEntity());
    }
    
    Coordinate FindCellToAddEntity() {
        Coordinate firstCell = new(0, 0);
        if (Entities.Count == 0)
            return firstCell;
        return Board.GetOppositeCoordinate(firstCell);
    }

    public void Move(IEntity entity, MoveDirection direction) {
        if (!EntityIsIntheBattle(entity.Identifier))
            throw new Exception($"The entity {entity.Identifier} is not in the board");
        Coordinate curentCellWithEntity = Board
            .GetEntityPosition(entity.Identifier);
        Coordinate targetCell = _GetTargetCellToMove(
            curentCellWithEntity, direction
        );
        Board.Move(entity.Identifier, targetCell);
    }


    Coordinate _GetTargetCellToMove(
        Coordinate sourceCell, MoveDirection direction
    ) {
        int x = sourceCell.X, y = sourceCell.Y;
        if (direction == MoveDirection.Up)
            y += 1;
        else if (direction == MoveDirection.Right)
            x += 1;
        else if (direction == MoveDirection.Down)
            y -= 1;
        else if (direction == MoveDirection.Left)
            x -= 1;
        Coordinate targetCell = new(x, y);
        if (!Board.IsCoordinateValid(targetCell))
            throw new Exception($"target cell ({x},{y}) not found.");
        return targetCell;
    }

    public void AddEntity(IEntity entity, Coordinate position)
    {
        Entities.Add(entity);
        Board.Place(entity.Identifier, position);
    }

    public bool EntityIsIntheBattle(string identifier) => 
        Board.GetEntities().Contains(identifier);
}