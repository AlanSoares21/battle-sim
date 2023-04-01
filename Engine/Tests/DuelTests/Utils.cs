using System;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Engine.Tests.DuelTests;

public static class Utils {
    public static IEntity FakeEntity(string id) {
        IEntity entity = A.Fake<IEntity>();
        A.CallTo(() => entity.Id).Returns(id);
        return entity;
    }

    public static IBattle CreateDuelWithEntities(params IEntity[] entities) 
    {
        IBattle duel = CreateDuel();
        foreach (var entity in entities)
            duel.AddEntity(entity);
        return duel;
    }

    public static IBattle CreateDuel() => 
        new Duel(Guid.NewGuid(), GameBoard.WithDefaultSize(), new Calculator());
}