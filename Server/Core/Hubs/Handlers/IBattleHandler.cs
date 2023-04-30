using BattleSimulator.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace BattleSimulator.Server.Hubs;

public interface IBattleHandler
{
    Task CreateDuel(string secondUser, CurrentCallerContext caller);
}