using System.Security.Claims;
using System.Text;
using BattleSimulator.Server.Database.Models;

namespace BattleSimulator.Server;

public class ServerConfig : IServerConfig
{
    private IConfiguration _configuration;
    public ServerConfig(IConfiguration configuration) {
        _configuration = configuration;
    }
    private int _SecondsAuthTokenExpireDefault = 60;
    private bool _SecondsAuthTokenExpireIsEmpty() => 
        string.IsNullOrEmpty(_configuration["SecondsAuthTokenExpire"]);
    public string Issuer => "" + _configuration["Jwt:Issuer"];

    public byte[] SecretKey => 
        Encoding.UTF8.GetBytes("" + _configuration["Jwt:Secret"]);

    public string Audience => "" + _configuration["Jwt:Audience"];

    public string ClaimTypeName => ClaimTypes.NameIdentifier;


    public int SecondsAuthTokenExpire => 
        _SecondsAuthTokenExpireIsEmpty() ?
        _SecondsAuthTokenExpireDefault :
        int.Parse("" + _configuration["SecondsAuthTokenExpire"]);

    public string AllowedOrigin => "" + _configuration["AllowedOrigin"];

    public string? DbFilePath => _configuration["DbFilePath"];

    public int ManaRecoveryWorkerIntervalInMiliseconds => 
        ManaRecoveryIntervalIsEmpty() ?
        _DefaultManaRecoveryInterval :
        int.Parse("" + _configuration["ManaRecoveryIntervalInMiliseconds"]);

    bool ManaRecoveryIntervalIsEmpty() => string.IsNullOrEmpty(_configuration["ManaRecoveryIntervalInMiliseconds"]);
    int _DefaultManaRecoveryInterval = 100;

    public int MovementWorkerIntervalInMiliseconds => 
        MovementIntervalIsEmpty() ?
        _DefaultMovementWorkerInterval :
        int.Parse("" + _configuration["MovementWorkerIntervalInMiliseconds"]);

    bool MovementIntervalIsEmpty() => 
        string.IsNullOrEmpty(_configuration["MovementWorkerIntervalInMiliseconds"]);
    
    int _DefaultMovementWorkerInterval = 100;

    public int IntervalToMoveEntitiesInSeconds => 
        IntervalToMoveEntitiesIsEmpty() ?
        _DefaultIntervalToMoveEntities
        :
        int.Parse("" + _configuration["IntervalToMoveEntitiesInSeconds"]);

    bool IntervalToMoveEntitiesIsEmpty() => 
        string.IsNullOrEmpty(_configuration["IntervalToMoveEntitiesInSeconds"]);
    
    int _DefaultIntervalToMoveEntities = 2;

    public Entity DefaultEntity(string id) => new Entity() {
        Id = id,
        Damage = 10,
        DefenseAbsorption = 0.1,
        HealthRadius = 50,
        MaxMana = 50,
        Skills = new List<string>() {
            "basicNegativeDamageOnX",
            "basicNegativeDamageOnY",
            "basicPositiveDamageOnX",
            "basicPositiveDamageOnY"
        }
    };
}