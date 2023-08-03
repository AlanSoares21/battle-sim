using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Workers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests.Builders;

public class ManaRecoveryWorkerBuilder
{
    ILogger<ManaRecoveryWorker>? _logger;
    IServerConfig? _config;
    IHubContext<GameHub, IGameHubClient>? _hub;
    IBattleCollection? _battles;
    
    public ManaRecoveryWorkerBuilder WithConfig(IServerConfig value) {
        _config = value;
        return this;
    }

    public ManaRecoveryWorkerBuilder WithHub(IHubContext<GameHub, IGameHubClient> hub) {
        _hub = hub;
        return this;
    }

    public ManaRecoveryWorkerBuilder WithBattleCollection(IBattleCollection value) {
        _battles = value;
        return this;
    }

    public ManaRecoveryWorker Build() {
        if (_config is null)
            _config = FakeServerConfig();
        if (_hub is null)
            _hub = A.Fake<IHubContext<GameHub, IGameHubClient>>();
        if (_battles is null)
            _battles = A.Fake<IBattleCollection>();
        return new ManaRecoveryWorker(
            A.Fake<ILogger<ManaRecoveryWorker>>(), 
            _config,
            _hub,
            _battles
        );
    }

    public static IServerConfig FakeServerConfig() {
        var config = A.Fake<IServerConfig>();
        A.CallTo(() => config.ManaRecoveryWorkerIntervalInMiliseconds).Returns(1);
        return config;
    }
}