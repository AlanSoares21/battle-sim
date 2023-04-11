using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Tests.Builders;
using Microsoft.Extensions.Logging;
using BattleSimulator.Engine.Skills;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleEvents;

[TestClass]
public class SkillTests 
{

    [TestMethod]
    public async Task Notify_Skill_Damage_Event_To_Users() 
    {
        var client = A.Fake<IGameHubClient>();
        var skill = new BasicNegativeDamageOnX();
        IEntity callerEntity = Utils.FakeEntity("callerId", new() { skill });
        IEntity requesterEntity = Utils.FakeEntity("requesterId");
        CurrentCallerContext caller = new(
            callerEntity.Id, 
            "callerConnectionId",
            Utils.FakeHubCallerContext(client));
        BattleRequest request = new() {
            requester = requesterEntity.Id,
            target = caller.UserId
        };
        IGameHubState state = FunctionalState();
        state.BattleRequests.TryAdd(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .WithDb(Utils.FakeDbWithEntities(callerEntity, requesterEntity))
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        await engine.Skill(skill.Name, requesterEntity.Id, caller);
        A.CallTo(() => client.Skill(
            skill.Name, 
            callerEntity.Id, 
            requesterEntity.Id,
            A<Coordinate>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    IGameHubState FunctionalState() 
    {
        return new GameHubStateBuilder()
            .WithBattleCollection(new BattleCollection())
            .WithBattleRequestCollection(
                new BattleRequestCollection(A.Fake<ILogger<BattleRequestCollection>>()
            ))
            .Build();
    }
}