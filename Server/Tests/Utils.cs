using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Server.Tests;

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

    public static IEntity FakeEntity(string id) 
    {
        var entity = A.Fake<IEntity>();
        A.CallTo(() => entity.Id).Returns(id);
        return entity;
    }
}