using BattleSimulator.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests.Builders;

public class RequestsHandlerBuilder
{
    IBattleRequestCollection? _Requests;
    IBattleHandler? _BattleHandler;

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

    public IRequestsHandler Build()
    {
        if (_Requests is null)
            _Requests = FakeRequestCollection();
        if (_BattleHandler is null)
            _BattleHandler = A.Fake<IBattleHandler>();
        return new RequestsHandler(
            _Requests, 
            _BattleHandler, 
            A.Fake<ILogger<RequestsHandler>>());
    }

    IBattleRequestCollection FakeRequestCollection() {
        var collection = A.Fake<IBattleRequestCollection>();
        A.CallTo(collection)
            .WithReturnType<bool>()
            .Returns(true);
        return collection;
    }
}