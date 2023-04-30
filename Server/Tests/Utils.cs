using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Server.Tests.Builders;
using BattleSimulator.Server.Models;

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

    public static IHubCallerClients<IGameHubClient> FakeHubContextWithClientForUser(
        string user,
        IGameHubClient client)
    {
        var context = A.Fake<IHubCallerClients<IGameHubClient>>();
        AddClientForUser(context, user, client);
        return context;
    }

    public static void AddClientForUser(
        IHubCallerClients<IGameHubClient> context,
        string user,
        IGameHubClient client)
    {
        A.CallTo(() => context.User(user)).Returns(client);
    }

    public static IHubCallerClients<IGameHubClient> FakeHubContextWithClientForCaller(
        IGameHubClient client)
    {
        var context = A.Fake<IHubCallerClients<IGameHubClient>>();
        A.CallTo(() => context.Caller).Returns(client);
        return context;
    }

    public static IGroupManager FakeGroupManager() => 
        A.Fake<IGroupManager>();

    public static IEntity FakeEntity(string id, List<ISkillBase> skills) 
    {
        var entity = FakeEntity(id);
        A.CallTo(() => entity.Skills).Returns(skills);
        return entity;
    }

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
            A.Fake<ICalculator>(),
            A.Fake<IEventsObserver>());

    public static List<ISkillBase> NewSkillSet(params string[] names) {
        List<ISkillBase> skills = new();
        foreach (var name in names)
        {
            skills.Add(FakeSkill(name));
        }
        return NewSkillSet(skills.ToArray());
    }

    public static ISkillBase FakeSkill(string name) 
    {
        var skill = A.Fake<ISkillBase>();
        A.CallTo(() => skill.Name).Returns(name);
        return skill;
    }

    public static List<ISkillBase> NewSkillSet(params ISkillBase[] skills) {
        List<ISkillBase> value = new();
        foreach (var skill in skills)
        {
            value.Add(skill);
        }
        return value;
    }

    public static IGameHubState FakeStateWithRequest(BattleRequest request) {
        IGameHubState state = new GameHubStateBuilder().Build();
        AddRequestOnState(state, request);
        return state;
    }

    public static void AddRequestOnState(IGameHubState state, BattleRequest request) {
        A.CallTo(() => state.BattleRequests.Get(request.requestId))
            .Returns(request);
    }

    public static IBattleRequestCollection FakeBattleRequestCollection(params BattleRequest[] requests)
    {
        var collection = A.Fake<IBattleRequestCollection>(); 
        foreach (var request in requests)
            AddRequestOnCollection(collection, request);
        return collection;
    }

    public static void AddRequestOnCollection(
        IBattleRequestCollection collection, 
        BattleRequest request) 
    {
        A.CallTo(() => collection.Get(request.requestId))
            .Returns(request);
    }

    public static void EnableRemoveRequest(IBattleRequestCollection collection, BattleRequest request)
    {
        A.CallTo(() => collection.TryRemove(request)).Returns(true);
    }

    public static IGameDb FakeDbWithEntities(params IEntity[] entities) {
        IGameDb gameDb = A.Fake<IGameDb>();
        foreach (var entity in entities)
            A.CallTo(() => gameDb.SearchEntity(entity.Id))
                .Returns(entity);
        return gameDb;
    }
}