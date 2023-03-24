using BattleSimulator.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleEvents;

[TestClass]
public class AttacksTests {
    
    [TestMethod]
    public void Register_Attacks() {
        string callerId = "callerId";
        string targetId = "targetId";
        var attacksRequested = A.Fake<IAttacksRequestedList>();
        IBattleEventsHandler eventsHandler = CreateEventsHandler(attacksRequested);
        eventsHandler.Attack(targetId, callerId);
        A.CallTo(() => attacksRequested.RegisterAttack(callerId, targetId))
            .MustHaveHappenedOnceExactly();
    }

    IBattleEventsHandler CreateEventsHandler(
        IAttacksRequestedList attacksRequestedList) => 
        new BattleEventsHandler(
            attacksRequestedList,
            A.Fake<ILogger<BattleEventsHandler>>());
}