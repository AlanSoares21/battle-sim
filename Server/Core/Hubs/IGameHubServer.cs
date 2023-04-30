namespace BattleSimulator.Server.Hubs;

public interface IGameHubServer
{
    Task ListUsers();
    Task SendBattleRequest(string targetUsername);
    Task AcceptBattle(Guid requestId);
    Task Move(int x, int y);
    Task CancelBattleRequest(Guid requesterId);
    Task CancelBattle(Guid battleId);
    void Attack(string targetId);
    void Skill(string skillName, string targetId);
}