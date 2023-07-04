using System;
using System.Collections.Generic;
using BattleSimulator.Engine.Equipment;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;

namespace BattleSimulator.Engine.Interfaces;

public interface IBattle
{
    Guid Id { get; }
    IBoard Board { get; }
    List<IEntity> Entities { get; }
    IEventsObserver Notify { get; }
    bool EntityIsIntheBattle(string identifier);
    void AddEntity(IEntity entity);
    void AddEntity(IEntity entity, Coordinate position);
    void Move(IEntity entity, MoveDirection direction);
    void Attack(string targetId, string attackerId);
    void DealDamage(
        int damage, 
        IOffensiveAttributes attackerAttributes,
        IEntity target,
        DamageDirection damageOnX,
        DamageDirection damageOnY);
    bool CanAttack(string targetId, string attackerId);
}