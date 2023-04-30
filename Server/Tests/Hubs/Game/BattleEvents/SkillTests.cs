using BattleSimulator.Engine;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Tests.Builders;
using Microsoft.Extensions.Logging;
using BattleSimulator.Engine.Skills;
using BattleSimulator.Server.Hubs.EventHandling;
using BattleSimulator.Server.Database;
using BattleSimulator.Engine.Interfaces.Skills;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleEvents;

[TestClass]
public class SkillTests 
{
    [TestMethod]
    public void Ensure_Is_Calling_Skill_Exec_Method()
    {
        var skill = Utils.FakeSkill("someSkillName");
        IEntity target = Utils.FakeEntity("targetId");
        IEntity caller = Utils.FakeEntity("callerId", new() { skill });
        CurrentCallerContext callerContext = new(
            caller.Id, 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        IBattleCollection battles = 
            Utils.BattleCollectionWithBattleFor(caller, target);

        IBattleEventsHandler handler = new BattleEventsHandlerBuilder()
            .WithBattles(battles)
            .Build();

        handler.Skill(skill.Name, target.Id, callerContext);

        ShouldCallExecMethod(skill, target, caller);
    }

    void ShouldCallExecMethod(ISkillBase skill, IEntity target, IEntity caller)
    {
        A.CallTo(() => skill.Exec(target, caller, A<IBattle>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}