using System.Collections.Concurrent;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public class BattleRequestCollection : IBattleRequestCollection
{
    ConcurrentDictionary<Guid, BattleRequest> _battleRequests;
    ILogger<BattleRequestCollection> _logger;
    public BattleRequestCollection(ILogger<BattleRequestCollection> logger) {
        _battleRequests = new();
        _logger = logger;
    }

    public BattleRequest Get(Guid requestId) => _battleRequests[requestId];

    public IEnumerable<BattleRequest> List() => _battleRequests.Values;

    public IList<BattleRequest> RequestsWithUser(string userIdentifier) => 
        List()
        .Where(request => request.UserIsOnRequest(userIdentifier))
        .ToList();

    public bool TryAdd(BattleRequest newBattleRequest) => 
        _battleRequests.TryAdd(newBattleRequest.requestId, newBattleRequest);


    public bool TryRemove(BattleRequest request) =>
        _battleRequests.TryRemove(new(request.requestId, request));
    
}