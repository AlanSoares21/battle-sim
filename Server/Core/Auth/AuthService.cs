using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

using System.Security.Cryptography;
using BattleSimulator.Server.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BattleSimulator.Server.Auth;

public class AuthService: IAuthService
{
    IServerConfig _serverConfig;
    IGameHubState _gameHubState;
    
    IDistributedCache _cache;

    public AuthService(
        IServerConfig serverConfig, 
        IGameHubState hubState,
        IDistributedCache cache) {
        _serverConfig = serverConfig;
        _gameHubState = hubState;
        _cache = cache;
    }

    public bool NameIsBeingUsed(string username) => _gameHubState.Connections
        .UsersIds()
        .Any(name => name == username);

    public async Task<(string accessToken, string refreshToken)> GenerateTokens(
        string username
    )
    {
        var accessToken = GenerateJwtToken(username);
        string refreshToken = GenerateRefreshToken();
        UserAuthenticated user = new() {
            Id = username,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.Now.AddHours(1)
        };
        await _cache.SetStringAsync(user.Id, JsonSerializer.Serialize(user));
        return (accessToken, refreshToken);
    }

    string GenerateJwtToken(string username)
    {
        var claims = new Claim[] { 
            new(_serverConfig.ClaimTypeName, username) 
        };
        return new JwtSecurityTokenHandler().WriteToken(JwtToken(claims));
    }

    JwtSecurityToken JwtToken(Claim[] claims) => new JwtSecurityToken(
        _serverConfig.Issuer, 
        _serverConfig.Audience, 
        claims, 
        expires: DateTime.UtcNow
            .AddSeconds(_serverConfig.SecondsAuthTokenExpire), 
        signingCredentials: GetSigningCredentials()
    );

    SigningCredentials GetSigningCredentials() => new SigningCredentials(
        new SymmetricSecurityKey(_serverConfig.SecretKey), 
        SecurityAlgorithms.HmacSha256Signature
    );

    private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

}