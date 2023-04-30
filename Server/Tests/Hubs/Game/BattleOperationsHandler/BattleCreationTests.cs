using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Tests.Builders;
using Microsoft.AspNetCore.SignalR;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleOperationsHandler;

[TestClass]
public class BattleCreationTests
{
    [TestMethod]
    public async Task Add_Battle_In_Battle_Collection()
    {
        CurrentCallerContext caller = new() {
            UserId = "callerId",
            HubClients = Utils.FakeHubCallerContext()
        };
        var battleCollection = A.Fake<IBattleCollection>();

        IBattleHandler handler = new BattleHandlerBuilder()
            .WithBattleCollection(battleCollection)
            .Build();
        await handler.CreateDuel("secondUser", caller);

        BattleAddShouldHappen(battleCollection);
    }

    void BattleAddShouldHappen(IBattleCollection collection)
    {
        A.CallTo(() => collection.TryAdd(A<IBattle>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
    
    [TestMethod]
    public async Task Battle_Created_Has_User_Entities_From_Db()
    {
        IEntity caller = Utils.FakeEntity("callerId"); 
        IEntity otherUser = Utils.FakeEntity("caller");
        CurrentCallerContext callerContext = new() {
            UserId = caller.Id,
            HubClients = Utils.FakeHubCallerContext()
        };
        var db = Utils.FakeDbWithEntities(caller, otherUser);
        var battleCollection = A.Fake<IBattleCollection>();

        IBattleHandler handler = new BattleHandlerBuilder()
            .WithDb(db)
            .WithBattleCollection(battleCollection)
            .Build();
        await handler.CreateDuel(otherUser.Id, callerContext);

        BattleAddedHadEntities(battleCollection, caller, otherUser);       
    }

    void BattleAddedHadEntities(IBattleCollection collection, params IEntity[] entities) 
    {
        A.CallTo(() => collection.TryAdd(EntitiesAreInBattle(entities))
            ).MustHaveHappenedOnceExactly();
    }

    IBattle EntitiesAreInBattle(IEntity[] entities) {
        return A<IBattle>.That.Matches(b => 
            entities.All(e => b.EntityIsIntheBattle(e.Id)));
    }

    [TestMethod]
    public async Task Create_Hub_Group_With_Users()
    {
        string otherUserId = "otherId";
        string otherUserConnectionId = "otherConnectionId" ;
        CurrentCallerContext caller = new() {
            UserId = "callerId",
            ConnectionId = "callerConnectionId",
            HubClients = Utils.FakeHubCallerContext()
        };
        var context = A.Fake<IHubContext<GameHub, IGameHubClient>>();
        var userMap = ConnMapWith((otherUserId, otherUserConnectionId));

        IBattleHandler handler = new BattleHandlerBuilder()
            .WithHubContext(context)
            .WithConnectionMapping(userMap)
            .Build();
        await handler.CreateDuel(otherUserId, caller);

        GroupWithConnectionsWasCreated(
            context, 
            caller.ConnectionId, 
            otherUserConnectionId);
    }

    IConnectionMapping ConnMapWith(params (string, string)[] connections)
    {
        var connectionsMap = A.Fake<IConnectionMapping>();
        foreach (var connection in connections)
        {
            A.CallTo(() => connectionsMap.GetConnectionId(connection.Item1))
                .Returns(connection.Item2);
        }
        return connectionsMap;
    }

    BattleRequest RequestWith(string requester, string target)
    {
        BattleRequest request = new();
        request.requester = requester;
        request.target = target;
        return request;
    }

    void GroupWithConnectionsWasCreated(
        IHubContext<GameHub, IGameHubClient> context, 
        params string[] connectionIds)
    {
        foreach (var connectionId in connectionIds)
        {
            A.CallTo(() => 
                context.Groups.AddToGroupAsync(
                    connectionId, 
                    A<string>.Ignored, 
                    A<CancellationToken>.Ignored
                )
            )
            .MustHaveHappenedOnceExactly();
        }
    }

    // TODO:: notify the group about the new battle
}