using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Tests.Builders;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleOperationsHandler;

[TestClass]
public class BattleCreationTests
{
    // TODO:: create object in battle colletion
    // TODO:: create hub clients group with the users
    // TODO:: notify the group about the new battle
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
        handler.CreateDuel(new(), caller);

        A.CallTo(() => battleCollection.TryAdd(A<IBattle>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    // TODO:: check if the user entities are present on the battle created
}