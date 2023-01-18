using BattleSimulator.Server.Tests.Builders;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Tests.Hubs.Game.Engine;

[TestClass]
public class ConnectionTests
{
    [TestMethod]
    public async Task Remove_Battle_Requests_When_User_Disconnect() {
        string userId = "callerId";
        string connectionId = "callerConnectionId";
        CurrentCallerContext callerContext = new(
            userId,
            connectionId,
            Utils.FakeHubCallerContext());
        var requestsWithUser = new List<BattleRequest>() {
            new() { 
                requestId = Guid.NewGuid(),
                requester = userId
            },
            new() { 
                requestId = Guid.NewGuid(),
                target = userId
            }
        };
        var requestCollection = A.Fake<IBattleRequestCollection>();
        A.CallTo(() => 
            requestCollection.RequestsWithUser(userId))
            .Returns(requestsWithUser);
        IGameHubState state = new GameHubStateBuilder()
            .WithBattleRequestCollection(requestCollection)
            .Build();
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.HandleUserDisconnected(callerContext);
        A.CallTo(() => requestCollection.TryRemove(requestsWithUser[0]))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => requestCollection.TryRemove(requestsWithUser[1]))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Remove_Battle_When_User_Disconnect() {
        string userId = "callerId";
        CurrentCallerContext callerContext = new(
            userId,
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        IGameHubState state = new GameHubStateBuilder().Build();
        IBattle battle = A.Fake<IBattle>();
        A.CallTo(() => state.Battles.GetBattleIdByEntity(userId))
            .Returns(battle.Id);
        A.CallTo(() => state.Battles.Get(battle.Id)).Returns(battle);
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.HandleUserDisconnected(callerContext);
        A.CallTo(() => state.Battles.TryRemove(battle))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Remove_Connection_Mapping_When_User_Disconnect() {
        string userId = "callerId";
        string connectionId = "callerConnectionId";
        CurrentCallerContext callerContext = new(
            userId,
            connectionId,
            Utils.FakeHubCallerContext());
        var connectionMapping = A.Fake<IConnectionMapping>();
        A.CallTo(connectionMapping)
            .WithReturnType<bool>()
            .Returns(true);
        IGameHubState state = new GameHubStateBuilder()
            .WithConnectionMapping(connectionMapping)
            .Build();
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.HandleUserDisconnected(callerContext);
        A.CallTo(() => connectionMapping.TryRemove(userId, connectionId))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Add_Connection_Mapping_When_User_Connect() {
        string userId = "callerId";
        string connectionId = "callerConnectionId";
        CurrentCallerContext callerContext = new(
            userId,
            connectionId,
            Utils.FakeHubCallerContext());
        var connectionMapping = A.Fake<IConnectionMapping>();
        A.CallTo(connectionMapping)
            .WithReturnType<bool>()
            .Returns(true);
        IGameHubState state = new GameHubStateBuilder()
            .WithConnectionMapping(connectionMapping)
            .Build();
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.HandleUserConnected(callerContext);
        A.CallTo(() => connectionMapping.TryAdd(userId, connectionId))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task Throws_Exception_When_Fail_Adding_Connection() {
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext());
        var connectionMapping = A.Fake<IConnectionMapping>();
        A.CallTo(connectionMapping)
            .WithReturnType<bool>()
            .Returns(false);
        IGameHubState state = new GameHubStateBuilder()
            .WithConnectionMapping(connectionMapping)
            .Build();
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await Assert.ThrowsExceptionAsync<Exception>(() => 
            engine.HandleUserConnected(callerContext));
    }

    [TestMethod]
    public async Task Notify_Users_When_Someone_Disconnect() {
        var client = A.Fake<IGameHubClient>();
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext(client));
        IGameEngine engine = new GameEngineBuilder()
            .Build();
        await engine.HandleUserDisconnected(callerContext);
        A.CallTo(() => client.UserDisconnect(new()))
            .WithAnyArguments()
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task Notify_Users_When_Someone_Connect() {
        var client = A.Fake<IGameHubClient>();
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext(client));
        IGameEngine engine = new GameEngineBuilder().Build();
        await engine.HandleUserConnected(callerContext);
        A.CallTo(() => client.UserConnect(new()))
            .WithAnyArguments()
            .MustHaveHappenedOnceOrMore();
    }
}