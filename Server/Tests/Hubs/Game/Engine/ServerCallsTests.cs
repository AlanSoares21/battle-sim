using BattleSimulator.Server.Tests.Builders;
using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Tests.Hubs.Game.Engine;

[TestClass]
public class ServerCallsTests
{
    [TestMethod]
    public async Task List_Users() {
        var client = A.Fake<IGameHubClient>();
        CurrentCallerContext callerContext = new(
            "callerId",
            "callerConnectionId",
            Utils.FakeHubCallerContext(client));
        IGameHubState state = new GameHubStateBuilder().Build();
        IGameEngine engine = new GameEngineBuilder()
            .WithState(state)
            .Build();
        await engine.ListUsers(callerContext);
        A.CallTo(() => state.Connections.UsersIds())
            .MustHaveHappened();
        A.CallTo(() => 
            client.ListConnectedUsers(new List<UserConnected>()))
                .WithAnyArguments()
                .MustHaveHappened();
    }

    [TestMethod]
    public async Task When_List_Users_Dont_List_The_Current_User() {
        string currentUserId = "callerId";
        var client = A.Fake<IGameHubClient>();
        CurrentCallerContext callerContext = new(
            currentUserId,
            "callerConnectionId",
            Utils.FakeHubCallerContext(client));
        List<string> connectedUsers = new() {
            "someUserId",
            currentUserId,
            "anotherUserId"
        };
        IGameHubState state = FakeStateWithUsers(connectedUsers);
        IGameEngine engine = new GameEngineBuilder().Build();
        await engine.ListUsers(callerContext);
        A.CallTo(() => 
            client.ListConnectedUsers(UserIsentInTheList(currentUserId)))
                .MustHaveHappened();
    }

    IGameHubState FakeStateWithUsers(List<string> usersIds) {
        IGameHubState state = new GameHubStateBuilder().Build();
        A.CallTo(() => state.Connections.UsersIds())
            .Returns(usersIds);
        return state;
    }

    IEnumerable<UserConnected> UserIsentInTheList(string userId) {
        return A<IEnumerable<UserConnected>>.That.Matches(users => 
            !users.Any(user => user.name == userId));
    }
}