using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleSimulator.Engine.Equipment;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;

namespace BattleSimulator.Engine;

public class Duel : IBattle
{
    Dictionary<string, Coordinate> _moveIntentions;
    ICalculator _calc;
    public Duel(
        Guid battleId, 
        IBoard board, 
        ICalculator calculator,
        IEventsObserver notifier) 
    {
        _moveIntentions = new();
        this.Board = board;
        this.Entities = new();
        Id = battleId;
        _calc = calculator;
        Notify = notifier;
        ManaRecoveredAt = DateTime.MinValue;
    }
    public Guid Id { get; private set; }
    public List<IEntity> Entities { get; private set; }
    public IBoard Board { get; private set; }

    public IEventsObserver Notify { get; private set; }

    public DateTime ManaRecoveredAt { get; private set; }
    public DateTime EntitiesMovedAt { get; private set; }

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

    public void AddEntity(IEntity entity, Coordinate position)
    {
        Entities.Add(entity);
        Board.Place(entity.Id, position);
    }

    public bool EntityIsIntheBattle(string Id) => 
        Board.GetEntities().Contains(Id);

    public void Attack(string targetId, string attackerId) {
        if (!CanAttack(targetId, attackerId))
            return;
        ExecuteAttack(targetId, attackerId);
    }

    public bool CanAttack(string targetId, string attackerId) {
        Coordinate attakcerPosition = Board.GetEntityPosition(attackerId);
        Coordinate targetPosition = Board.GetEntityPosition(targetId);
        double xDiff = Math.Abs(targetPosition.X - attakcerPosition.X);
        if (xDiff > 1)
            return false;
        double yDiff = Math.Abs(targetPosition.Y - attakcerPosition.Y);
        return yDiff <= 1;
    }
    
    void ExecuteAttack(string targetId, string attackerId) {
        (IEntity target, IEntity attacker) = GetEntities(targetId, attackerId);
        DealDamage(
            attacker.OffensiveStats.Damage,
            attacker.OffensiveStats,
            target, 
            attacker.Weapon.damageOnX,
            attacker.Weapon.damageOnY);
    }

    public void DealDamage(
        int damage, 
        IOffensiveAttributes attackerAttributes,
        IEntity target, 
        DamageDirection damageOnX,
        DamageDirection damageOnY) 
    {
        damage = _calc.Damage(
            damage, 
            target.DefensiveStats.DefenseAbsorption, 
            attackerAttributes, 
            target.DefensiveStats);
        
        int xMul = 0;
        if (damageOnX == Equipment.DamageDirection.Positive)
            xMul = 1;
        else if (damageOnX == Equipment.DamageDirection.Negative)
            xMul = -1;

        int yMul = 0;
        if (damageOnY == Equipment.DamageDirection.Positive)
            yMul = 1;
        else if (damageOnY == Equipment.DamageDirection.Negative)
            yMul = -1;

        double newX = damage * xMul;
        double newY = damage * yMul;
        
        target.ApplyDamage(new(newX, newY));
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

    public Task RecoverMana()
    {
        foreach (var entity in Entities)
            RecoverManaForEntity(entity);
        ManaRecoveredAt = DateTime.UtcNow;
        return Notify.ManaRecovered();;
    }

    void RecoverManaForEntity(IEntity entity)
    {
        var diference = entity.State.MaxMana - entity.State.Mana;
        if (diference > 5)
            entity.State.Mana += 5;
        else if (diference > 0)
            entity.State.Mana += diference;
    }

    public void RegisterMove(string id, Coordinate moveTo)
    {
        if (!EntityIsIntheBattle(id))
            throw new Exception($"The entity {id} is not in the board");
        if (!Board.IsCoordinateValid(moveTo))
            throw new Exception($"Move to {moveTo} is invalid.");
        this._moveIntentions.Add(id, moveTo);
    }

    public async Task MoveEntities()
    {
        if (_moveIntentions.Keys.Count > 0) 
        {
            Dictionary<string, Coordinate> moved = new();
            foreach (var entityToMove in _moveIntentions.Keys)
                Move(entityToMove, ref moved);
            await Notify.Moved(moved);
        }
        EntitiesMovedAt = DateTime.UtcNow;
    }

    void Move(string id, ref Dictionary<string, Coordinate> moved) {
        Coordinate moveTo = _moveIntentions[id];
        Coordinate curentCell = Board.GetEntityPosition(id);
        Coordinate targetCell = _GetTargetCellToMove(curentCell, moveTo);
        Board.Move(id, targetCell);
        moved.Add(id, targetCell);
        if (targetCell.Equals(curentCell))
            _moveIntentions.Remove(id);
    }


    Coordinate _GetTargetCellToMove(
        Coordinate sourceCell, 
        Coordinate moveTo) 
    {
        Coordinate targetCell = new(sourceCell.X, sourceCell.Y);
        double diffX = moveTo.X - sourceCell.X;
        double diffY = moveTo.Y - sourceCell.Y;
        if (diffX != 0)
            if (diffX > 0)
                targetCell.X += 1;
            else 
                targetCell.X -= 1;
         
        if (diffY != 0) 
            if (diffY > 0)
                targetCell.Y += 1;
            else 
                targetCell.Y -= 1;
        return targetCell;
    }
}