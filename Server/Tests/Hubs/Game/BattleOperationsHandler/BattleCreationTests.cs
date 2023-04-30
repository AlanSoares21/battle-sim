using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Tests.Builders;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleOperationsHandler;

[TestClass]
public class BattleCreationTests
{
    [TestMethod]
    public void Add_Battle_In_Battle_Collection()
    {
        CurrentCallerContext caller = new() {
            UserId = "callerId",
            HubClients = Utils.FakeHubCallerContext()
        };
        var battleCollection = A.Fake<IBattleCollection>();

        IBattleHandler handler = new BattleHandlerBuilder()
            .WithBattleCollection(battleCollection)
            .Build();
        handler.CreateDuel(
            RequestWith("requesterId", caller.UserId), 
            caller);

        BattleAddShouldHappen(battleCollection);
    }

    void BattleAddShouldHappen(IBattleCollection collection)
    {
        A.CallTo(() => collection.TryAdd(A<IBattle>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
    
    [TestMethod]
    public void Battle_Created_Has_User_Entities_From_Db()
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
        handler.CreateDuel(
            RequestWith(otherUser.Id, caller.Id), 
            callerContext);

        BattleAddedHadEntities(battleCollection, caller, otherUser);       
    }

    BattleRequest RequestWith(string requester, string target)
    {
        BattleRequest request = new();
        request.requester = requester;
        request.target = target;
        return request;
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
    
    // TODO:: create hub clients group with the users
    // TODO:: notify the group about the new battle
}