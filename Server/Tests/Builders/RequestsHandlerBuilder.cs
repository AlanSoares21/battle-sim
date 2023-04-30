using BattleSimulator.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests.Builders;

public class RequestsHandlerBuilder
{
    IBattleRequestCollection? _Requests;
    IBattleHandler? _BattleHandler;
    IBattleCollection? _Battles;
    public RequestsHandlerBuilder WithRequestCollection(IBattleRequestCollection collection)
    {
        _Requests = collection;
        return this;
    }

    public RequestsHandlerBuilder WithBattleHandler(IBattleHandler handler)
    {
        _BattleHandler = handler;
        return this;
    }

    public RequestsHandlerBuilder WithBattleCollection(IBattleCollection collection)
    {
        _Battles = collection;
        return this;
    }

    public IRequestsHandler Build()
    {
        if (_Requests is null)
            _Requests = FakeRequestCollection();
        if (_BattleHandler is null)
            _BattleHandler = A.Fake<IBattleHandler>();
        if (_Battles is null)
            _Battles = FakeBattleCollection();
        return new RequestsHandler(
            _Requests, 
            _BattleHandler, 
            _Battles,
            A.Fake<ILogger<RequestsHandler>>());
    }

    IBattleCollection FakeBattleCollection()
    {
        var collection = A.Fake<IBattleCollection>();
        A.CallTo(() => collection.GetBattleIdByEntity(A<string>.Ignored))
            .Throws<KeyNotFoundException>();
        return collection;
    }

    IBattleRequestCollection FakeRequestCollection() {
        var collection = A.Fake<IBattleRequestCollection>();
        A.CallTo(collection)
            .WithReturnType<bool>()
            .Returns(true);
        return collection;
    }
}