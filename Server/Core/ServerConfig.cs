using System.Security.Claims;
using System.Text;

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
    public string Issuer => _configuration["Jwt:Issuer"];

    public byte[] SecretKey => 
        Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);

    public string Audience => _configuration["Jwt:Audience"];

    public string ClaimTypeName => ClaimTypes.NameIdentifier;


    public int SecondsAuthTokenExpire => 
        _SecondsAuthTokenExpireIsEmpty() ?
        _SecondsAuthTokenExpireDefault :
        int.Parse(_configuration["SecondsAuthTokenExpire"]);

    public string AllowedOrigin => _configuration["AllowedOrigin"];
}