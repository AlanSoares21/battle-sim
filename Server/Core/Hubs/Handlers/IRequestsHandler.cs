using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Hubs;

public interface IRequestsHandler
{
    Task SendTo(string target, CurrentCallerContext caller);
    void Accept(Guid requestId, CurrentCallerContext context);
}