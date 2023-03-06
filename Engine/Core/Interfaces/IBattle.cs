using System;
using System.Collections.Generic;

namespace BattleSimulator.Engine.Interfaces;

public interface IBattle
{
    Guid Id { get; }
    IBoard Board { get; }
    List<IEntity> Entities { get; }
    bool EntityIsIntheBattle(string identifier);
    void AddEntity(IEntity entity);
    void AddEntity(IEntity entity, Coordinate position);
    void Move(IEntity entity, MoveDirection direction);
    bool Attack(string targetId, string attackerId);
}