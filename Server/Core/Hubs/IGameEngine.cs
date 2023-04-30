using Microsoft.AspNetCore.SignalR;
using BattleSimulator.Engine;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public interface IGameEngine
{
    Task HandleUserDisconnected(CurrentCallerContext caller);
    Task HandleUserConnected(CurrentCallerContext caller);

    Task ListUsers(CurrentCallerContext caller);
    Task CancelBattleRequest(
        Guid requestId, 
        CurrentCallerContext caller);

    Task AcceptBattleRequest(
        Guid requestId, 
        CurrentCallerContext caller,
        IGroupManager groupManager);

    Task CancelBattle(Guid battleId, CurrentCallerContext caller);
    void Move(Coordinate coordinate, CurrentCallerContext caller);
}