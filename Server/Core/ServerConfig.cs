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

    public Entity DefaultEntity(string id) => new Entity() {
        Id = id,
        Damage = 10,
        DefenseAbsorption = 0.1,
        HealthRadius = 50,
        Skills = new List<string>() {
            "basicNegativeDamageOnX",
            "basicNegativeDamageOnY",
            "basicPositiveDamageOnX",
            "basicPositiveDamageOnY"
        }
    };
}