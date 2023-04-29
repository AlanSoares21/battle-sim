using BattleSimulator.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace BattleSimulator.Server.Tests.Builders;

public class RequestsHandlerBuilder
{
    IBattleRequestCollection? _Requests;

    public RequestsHandlerBuilder WithRequestCollection(IBattleRequestCollection collection)
    {
        _Requests = collection;
        return this;
    }

    public RequestsHandler Build()
    {
        if (_Requests is null)
            _Requests = FakeRequestCollection();
        return new(_Requests, A.Fake<ILogger<RequestsHandler>>());
    }

    IBattleRequestCollection FakeRequestCollection() {
        var collection = A.Fake<IBattleRequestCollection>();
        A.CallTo(collection)
            .WithReturnType<bool>()
            .Returns(true);
        return collection;
    }
}