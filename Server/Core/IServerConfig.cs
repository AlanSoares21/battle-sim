using BattleSimulator.Server.Database.Models;

namespace BattleSimulator.Server;

public interface IServerConfig
{
    string  ClaimTypeName { get; }
    int  SecondsAuthTokenExpire { get; }
    int  ManaRecoveryWorkerIntervalInMiliseconds { get; }
    int  MovementWorkerIntervalInMiliseconds { get; }
    string  Issuer { get; }
    string  Audience { get; }
    byte[] SecretKey  { get; }
    string  AllowedOrigin { get; }
    string? DbFilePath { get; }
    Entity DefaultEntity(string id);
}