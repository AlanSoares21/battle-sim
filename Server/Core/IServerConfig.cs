namespace BattleSimulator.Server;

public interface IServerConfig
{
    string  ClaimTypeName { get; }
    int  SecondsAuthTokenExpire { get; }
    string  Issuer { get; }
    string  Audience { get; }
    byte[] SecretKey  { get; }
    string  AllowedOrigin { get; }
}