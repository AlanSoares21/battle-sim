using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using BattleSimulator.Server.Models;

using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Auth;

public class AuthService: IAuthService
{
    private IServerConfig _serverConfig;
    private IGameHubState _gameHubState;
    public AuthService(IServerConfig serverConfig, IGameHubState hubState) {
        _serverConfig = serverConfig;
        _gameHubState = hubState;
    }
    public bool NameIsBeingUsed(string username) => _gameHubState.Connections
        .UsersIds()
        .Any(name => name == username);
    public string GenerateJwtToken(string username)
    {
        var key = _serverConfig.SecretKey;
        var claims = new Claim[] { 
            new(_serverConfig.ClaimTypeName, username) 
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key), 
            SecurityAlgorithms.HmacSha256Signature
        );
        var token = new JwtSecurityToken(
            _serverConfig.Issuer, 
            _serverConfig.Audience, 
            claims, 
            expires: DateTime.UtcNow
                .AddSeconds(_serverConfig.SecondsAuthTokenExpire), 
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}