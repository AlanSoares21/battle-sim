using BattleSimulator.Engine;
using BattleSimulator.Engine.Equipment;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine.Interfaces.Skills;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;
using BattleSimulator.Server.Tests.Builders;

namespace BattleSimulator.Server.Tests.Hubs.Game.Engine;

[TestClass]
public class BattleCreatedTests 
{
        [TestMethod]
    public async Task Create_Battle_When_Accept_A_Request() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = "requesterId",
            target = caller.UserId
        };
        IGameHubState state = Utils.FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => state.Battles.TryAdd(A<IBattle>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Place_Entities_In_The_Created_Battle() {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = "requesterId",
            target = caller.UserId
        };
        IGameHubState state = Utils.FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => state.Battles.TryAdd(UserIsInTheBattle(caller.UserId)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => state.Battles.TryAdd(UserIsInTheBattle(request.requester)))
            .MustHaveHappenedOnceExactly();
    }

    IBattle UserIsInTheBattle(string userId) => 
        A<IBattle>.That.Matches(battle => 
            battle.Entities.Any(e => e.Id == userId));

    [TestMethod]
    public async Task When_Create_Battle_And_Dont_Found_Entity_Data_On_Db_Use_Entity_With_Default_Data()
    {
        CurrentCallerContext caller = new(
            "callerId", 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = "requesterId",
            target = caller.UserId
        };
        IGameHubState state = Utils.FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .WithSkillProvider(new SkillProvider())
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => 
            state.Battles.TryAdd(
                BattleWithDefaultEntityForUser(caller.UserId)))
            .MustHaveHappened();
    }

    IBattle BattleWithDefaultEntityForUser(string userId) {
        return A<IBattle>.That.Matches(b => 
            b.Entities.Exists(e => 
                e.Id == userId && EntityIsDefault(e)));
    }

    bool EntityIsDefault(IEntity entity) =>
        entity.OffensiveStats.Damage == 10
        && entity.DefensiveStats.DefenseAbsorption == 0.1
        && entity.State.HealthRadius == 25
        && entity.State.CurrentHealth.CoordinatesAreEqual(25, 25)
        && WeaponIsDefault(entity.Weapon)
        && HadDefaultSkills(entity.Skills);
    

    bool WeaponIsDefault(Weapon weapon) =>
        weapon.damageOnX == DamageDirection.Positive
        && weapon.damageOnY == DamageDirection.Neutral;

    bool HadDefaultSkills(List<ISkillBase> skills) 
    {
        string[] defaultSkills = new[] {
            "basicNegativeDamageOnX"
        };
        int defaultSkillCount = 0;
        for (int i = 0; i < skills.Count; i++)
        {
            foreach(string defaultSkill in defaultSkills) {
                if (defaultSkill == skills[i].Name) {
                    defaultSkillCount++;
                    break;
                }
            }
        }
        return defaultSkillCount == defaultSkills.Length;
    }

    [TestMethod]
    public async Task When_Create_Battle_Get_Entity_Data_From_DB()
    {
        IEntity callerEntity = Utils.FakeEntity("callerId");
        IEntity requesterEntity = Utils.FakeEntity("requesterId");
        CurrentCallerContext caller = new(
            callerEntity.Id, 
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        BattleRequest request = new() {
            requester = requesterEntity.Id,
            target = caller.UserId
        };
        IGameHubState state = Utils.FakeStateWithRequest(request);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .WithDb(Utils.FakeDbWithEntities(callerEntity, requesterEntity))
            .Build();
        await engine.AcceptBattleRequest(
            request.requestId, 
            caller,
            Utils.FakeGroupManager());
        A.CallTo(() => 
            state.Battles.TryAdd(
                A<IBattle>.That.Matches(battle => 
                    EntitiesAreInBattle(battle, callerEntity, requesterEntity))))
            .MustHaveHappened();
    }

    bool EntitiesAreInBattle(IBattle battle, params IEntity[] entities) {
        foreach (var entity in entities)
        {
            if (!battle.Entities.Contains(entity))
                return false;
        }
        return true;
    }
}