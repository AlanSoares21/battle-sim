using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine;

namespace BattleSimulator.Server.Tests;

public static class Utils
{
    public static IHubCallerClients<IGameHubClient> FakeHubCallerContext(
        IGameHubClient client)
    {
        var callerClients = A.Fake<IHubCallerClients<IGameHubClient>>();
        A.CallTo(callerClients)
            .WithReturnType<IGameHubClient>()
            .Returns(client);
        return callerClients;
    }

    public static IHubCallerClients<IGameHubClient> FakeHubCallerContext() => 
        A.Fake<IHubCallerClients<IGameHubClient>>();

    public static IGroupManager FakeGroupManager() => 
        A.Fake<IGroupManager>();

    public static IEntity FakeEntity(string id) 
    {
        var entity = A.Fake<IEntity>();
        A.CallTo(() => entity.Id).Returns(id);
        return entity;
    }

    public static IBattleCollection BattleCollectionWithBattleFor(
        string firstEntityId, 
        string secondEntityId) 
    {
        return BattleCollectionWithBattleFor(
            Utils.FakeEntity(firstEntityId),
            Utils.FakeEntity(secondEntityId)
        );
    }
    
    public static IBattleCollection BattleCollectionWithBattleFor(
        IEntity firstEntity, 
        IEntity secondEntity) 
    {
        var battles = new BattleCollection();
        var battle = CreateDuel();
        battle.AddEntity(firstEntity, new(0, 0));
        battle.AddEntity(secondEntity, new(0, 0));
        battles.TryAdd(battle);
        return battles;
    }

    public static IBattle CreateDuel() =>
        new Duel(
            Guid.NewGuid(),
            GameBoard.WithDefaultSize(),
            A.Fake<ICalculator>());
}