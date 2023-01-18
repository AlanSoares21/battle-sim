using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Models;

public struct CurrentCallerContext {
    
    public CurrentCallerContext(
        string userId, 
        string connectionId,
        IHubCallerClients<IGameHubClient> callerClients) {
        UserId = userId;
        ConnectionId = connectionId;
        HubClients = callerClients;
    }

    public string UserId { get; set; }
    public string ConnectionId { get; set; }
    public IHubCallerClients<IGameHubClient> HubClients { get; set; }
    public IGameHubClient Connection { 
        get 
        {
            return HubClients.Caller;
        } 
    }
}