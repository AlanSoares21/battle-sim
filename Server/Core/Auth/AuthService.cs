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
        var accessToken = GetAccessToken(username);
        string refreshToken = GenerateRefreshToken();
        UserAuthenticated user = new() {
            Id = username,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.Now.AddHours(1)
        };
        await _cache.SetStringAsync(user.Id, JsonSerializer.Serialize(user));
        return (accessToken, refreshToken);
    }

    private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

    public async Task<string> NewAccessToken(string username, string refreshToken)
    {
        var json = await _cache.GetStringAsync(username);
        if (json is null || json.Length == 0)
            throw new KeyNotFoundException($"Not found key {username} in the cache");
        var user = JsonSerializer.Deserialize<UserAuthenticated>(json);
        if (user is null)
            throw new Exception($"On deserializing cache entry returned an null reference. cache entry: {json}. username: {username}");
        if (user.RefreshToken != refreshToken)
            throw new Exception($"refresh token {user.RefreshToken} registered to user {username} is different than {refreshToken}");
        if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
            throw new SecurityTokenExpiredException($"refresh token to user {username} expired {user.RefreshTokenExpiryTime}");
        return GetAccessToken(username);
    }

    public string GetAccessToken(string username)
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
        GetSecurityKey(), 
        SecurityAlgorithms.HmacSha256Signature
    );

    SymmetricSecurityKey GetSecurityKey() => 
        new SymmetricSecurityKey(_serverConfig.SecretKey);

    public string GetUsernameFromAccessToken(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(accessToken))
            throw new Exception($"Not possible read token {accessToken}");
        var validationParameters = GetValidationParameters();
        ClaimsPrincipal principal = tokenHandler.ValidateToken(
            accessToken, 
            validationParameters, 
            out SecurityToken token
        );
        Claim? claim = principal
            .FindFirst(c => c.Type == _serverConfig.ClaimTypeName);
        if (claim is null)
            throw new Exception($"{_serverConfig.ClaimTypeName} is null on token {accessToken}");
        return claim.Value;
    }

    TokenValidationParameters GetValidationParameters() =>
        new TokenValidationParameters() {
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateAudience = true,
            ValidAudience = _serverConfig.Audience,
            ValidIssuer = _serverConfig.Issuer,
            IssuerSigningKey = GetSecurityKey()
        };
}