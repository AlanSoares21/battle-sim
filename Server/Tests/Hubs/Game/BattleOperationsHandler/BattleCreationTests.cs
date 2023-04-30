using BattleSimulator.Engine;
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
        IEntity otherUser = Utils.FakeEntity("otherUserId");
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

    [TestMethod]
    public async Task Entities_Of_The_Users_Are_In_Battle_Data_Sent_To_The_Group()
    {
        IEntity caller = Utils.FakeEntity("callerId"); 
        IEntity otherUser = Utils.FakeEntity("otherUserId");
        var db = Utils.FakeDbWithEntities(caller, otherUser);
        CurrentCallerContext callerContext = new() {
            UserId = caller.Id,
            HubClients = Utils.FakeHubCallerContext()
        };
        var client = A.Fake<IGameHubClient>();
        ReturnClientForGroupCall(callerContext.HubClients, client);

        IBattleHandler handler = new BattleHandlerBuilder()
            .WithDb(db)
            .Build();
        await handler.CreateDuel(otherUser.Id, callerContext);

        NewBattleDataContainsEntities(client, caller, otherUser);
    }

    void NewBattleDataContainsEntities(IGameHubClient client, params IEntity[] entities)
    {
        A.CallTo(() => 
            client.NewBattle(EntitiesAreInBattleData(entities))
        ).MustHaveHappenedOnceExactly();
    }

    BattleData EntitiesAreInBattleData(IEntity[] entities)
    {
        return A<BattleData>.That.Matches(b => 
            b.entities.Count > 0
            && b.entities.All(e => entities.Contains(e)));
    }

    [TestMethod]
    public async Task Notify_New_Battle_To_The_Hub_Group()
    {
        var client = A.Fake<IGameHubClient>();
        CurrentCallerContext caller = new() {
            UserId = "callerId",
            HubClients = Utils.FakeHubCallerContext()
        };
        ReturnClientForGroupCall(caller.HubClients, client);

        IBattleHandler handler = new BattleHandlerBuilder().Build();
        await handler.CreateDuel("secondUser", caller);

        NewBattleEventShouldHappen(client);
    }

    void ReturnClientForGroupCall(
        IHubCallerClients<IGameHubClient> hubCaller, 
        IGameHubClient client)
    {
        A.CallTo(() => hubCaller.Group(A<string>.Ignored))
            .Returns(client);
    }

    void NewBattleEventShouldHappen(IGameHubClient client)
    {
        A.CallTo(() => client.NewBattle(A<BattleData>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Notify_Skill_Damage_Event_To_Users() 
    {
        string skillName = "someSkillName";
        string target = "targetOffSkill";
        Coordinate healthAfterSkill = new(0,0);
        IEntity callerEntity = Utils.FakeEntity("callerId");
        var client = A.Fake<IGameHubClient>();
        CurrentCallerContext caller = new(
            callerEntity.Id, 
            "callerConnectionId",
            Utils.FakeHubCallerContext(client));
        var battles = new BattleCollection();

        IBattleHandler battleHandler = new BattleHandlerBuilder()
            .WithDb(Utils.FakeDbWithEntities(callerEntity))
            .WithBattleCollection(battles)
            .Build();
        await battleHandler.CreateDuel("requesterEntityId", caller);
        var battle = battles.Get(battles.GetBattleIdByEntity(callerEntity.Id));
        battle.Notify.SkillDamage(
            skillName, 
            callerEntity.Id, 
            target, 
            healthAfterSkill);

        SkillCallHappened(client, skillName, callerEntity.Id, target);
    }

    void SkillCallHappened(
        IGameHubClient client, 
        string skillName, 
        string source, 
        string target) 
    {
        A.CallTo(() => 
            client.Skill(skillName, source, target, A<Coordinate>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}