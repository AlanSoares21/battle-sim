using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Tests.Hubs.Game.Engine;

public static class Utils
{
    public static IHubCallerClients<IGameHubClient> FakeHubCallerContext(
        IGameHubClient client)
    {
        var callerClients = A.Fake<IHubCallerClients<IGameHubClient>>();
        A.CallTo(callerClients)
            .WithReturnType<IGameHubClient>()
            .Returns(client);
        return callerClients;
    }

    public static IHubCallerClients<IGameHubClient> FakeHubCallerContext() => 
        A.Fake<IHubCallerClients<IGameHubClient>>();

    public static IGroupManager FakeGroupManager() => 
        A.Fake<IGroupManager>();
}