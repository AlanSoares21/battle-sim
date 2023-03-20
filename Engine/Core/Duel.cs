using System;
using System.Collections.Generic;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Engine;

public class Duel : IBattle
{
    private ICalculator Calc;
    public Duel(Guid battleId, IBoard board, ICalculator calculator) {
        this.Board = board;
        this.Entities = new();
        Id = battleId;
        Calc = calculator;
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
        if (!EntityIsIntheBattle(entity.Id))
            throw new Exception($"The entity {entity.Id} is not in the board");
        Coordinate curentCellWithEntity = Board
            .GetEntityPosition(entity.Id);
        Coordinate targetCell = _GetTargetCellToMove(
            curentCellWithEntity, direction
        );
        Board.Move(entity.Id, targetCell);
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
        Board.Place(entity.Id, position);
    }

    public bool EntityIsIntheBattle(string Id) => 
        Board.GetEntities().Contains(Id);

    public bool Attack(string targetId, string attackerId) {
        if (!CanAttack(targetId, attackerId))
            return false;
        ExecuteAttack(targetId, attackerId);
        return true;
    }

    bool CanAttack(string targetId, string attackerId) {
        Coordinate attakcerPosition = Board.GetEntityPosition(attackerId);
        Coordinate targetPosition = Board.GetEntityPosition(targetId);
        int xDiff = Math.Abs(targetPosition.X - attakcerPosition.X);
        if (xDiff > 1)
            return false;
        int yDiff = Math.Abs(targetPosition.Y - attakcerPosition.Y);
        return yDiff <= 1;
    }

    void ExecuteAttack(string targetId, string attackerId) {
        (IEntity target, IEntity attacker) = GetEntities(targetId, attackerId);
        int damage = Calc
            .Damage(attacker.Damage, target.DefenseAbsorption, attacker, target);
        
        int xMul = 0;
        if (attacker.Weapon.damageOnX == Equipment.DamageDirection.Positive)
            xMul = 1;
        else if (attacker.Weapon.damageOnX == Equipment.DamageDirection.Negative)
            xMul = -1;

        int yMul = 0;
        if (attacker.Weapon.damageOnY == Equipment.DamageDirection.Positive)
            yMul = 1;
        else if (attacker.Weapon.damageOnY == Equipment.DamageDirection.Negative)
            yMul = -1;

        int newX = target.CurrentHealth.X + damage * xMul;
        int newY = target.CurrentHealth.Y + damage * yMul;
        target.CurrentHealth = new(newX, newY);
    }

    (IEntity target, IEntity attacker) GetEntities(string targetId, string attackerId) {
        IEntity target, attacker;
        if (Entities[0].Id == targetId) {
            target = Entities[0];
            attacker = Entities[1];
        } else {
            target = Entities[1];
            attacker = Entities[0];
        }
        return new(target, attacker);
    }
}