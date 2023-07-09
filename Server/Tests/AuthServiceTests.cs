using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using BattleSimulator.Server.Auth;
using BattleSimulator.Server.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BattleSimulator.Server.Tests;

[TestClass]
public class AuthServiceTests
{
    [TestMethod]
    public void Return_True_When_Some_User_Has_Been_Connected_With_The_Same_Name() {
        const string nameUsed = "name_used";
        var namesUsedList = new List<string>() { nameUsed };
        IAuthService authService = CreateAuthServiceWithThisUsersConnected(namesUsedList);
        Assert.IsTrue(authService.NameIsBeingUsed(nameUsed));
    }

    [TestMethod]
    public void Return_False_When_No_Users_Has_Been_Connected_With_The_Same_Name() {
        const string nameNotUsed = "name_not_used";
        var namesUsedList = new List<string>();
        IAuthService authService = CreateAuthServiceWithThisUsersConnected(namesUsedList);
        Assert.IsFalse(authService.NameIsBeingUsed(nameNotUsed));
    }

    IAuthService CreateAuthServiceWithThisUsersConnected(List<string> usersConnected) {
        IServerConfig serverConfig = A.Fake<IServerConfig>();
        IGameHubState gameHubState = A.Fake<IGameHubState>();
        A.CallTo(() => gameHubState.Connections.UsersIds()).Returns(usersConnected);
        return new AuthService(
            serverConfig, 
            gameHubState, 
            A.Fake<IDistributedCache>());
    }

    [TestMethod]
    public async Task The_Server_Config_Is_Being_Used() 
    {
        const string username = "username";
        IServerConfig serverConfig = FakeServerConfig();
        var authService = CreateAuthServiceWithThisServerConfig(serverConfig);
        await authService.GenerateTokens(username);
        A.CallTo(() => serverConfig.Audience).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.ClaimTypeName).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.Issuer).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.SecondsAuthTokenExpire).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.SecretKey).MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task Generate_Valid_JWT_Token() 
    {
        const string username = "username";
        IServerConfig serverConfig = FakeServerConfig();
        var authService = CreateAuthServiceWithThisServerConfig(serverConfig);
        var (jwtToken, _) = await authService.GenerateTokens(username);
        JwtTokenCanBeRead(jwtToken);
    }

    [TestMethod]
    public async Task Generate_JWT_Token_With_The_ClaimIndentity_Of_Server_Config() 
    {
        const string username = "username";
        IServerConfig serverConfig = FakeServerConfig();
        string claimTypeName = "SomeValuToClaimTypeName";
        A.CallTo(() => serverConfig.ClaimTypeName).Returns(claimTypeName);
        var authService = CreateAuthServiceWithThisServerConfig(serverConfig);
        var (jwtToken, _) = await authService.GenerateTokens(username);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtTokenData = tokenHandler.ReadJwtToken(jwtToken);
        Assert.IsTrue(
            jwtTokenData.Claims
                .Any(claim => claim.Type == claimTypeName)
        );
    }

    IAuthService CreateAuthServiceWithThisServerConfig(IServerConfig serverConfig) {
        IGameHubState gameHubState = A.Fake<IGameHubState>();
        return new AuthService(
            serverConfig, 
            gameHubState, 
            A.Fake<IDistributedCache>());
    }

    [TestMethod]
    public async Task Register_Refresh_Token_On_Cache()
    {
        const string username = "username";
        var cache = CreateCache();
        var authService = CreateAuthServiceWithCache(cache);
        var tokens = await authService.GenerateTokens(username);
        var jsonRegister = cache.GetString(username);
        Assert.IsNotNull(jsonRegister, "cache dont have a register with username as key");
        var register = JsonSerializer
            .Deserialize<UserAuthenticated>(jsonRegister);
        Assert.IsNotNull(register, $"The json information stored in the cache dont match the model. {jsonRegister}");
        Assert.IsNotNull(register.RefreshToken, "The refresh token stored in the cache is null");
        Assert.AreEqual(
            register.RefreshToken, 
            tokens.refreshToken, 
            $"The refresh token stored in the cahce is different than the token returned from the method. cache: {register.RefreshToken} - returned: {tokens.refreshToken}"
        );
    }

    [TestMethod]
    public async Task Throw_Exception_When_Try_Get_New_Access_Token_To_An_Unregistered_User()
    {
        string unregisteredUsername = "unregistered";
        var authService = CreateAuthServiceWithCache(
            A.Fake<IDistributedCache>()
        );
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() =>
            authService.NewAccessToken(unregisteredUsername, "")
        );
    }

    [TestMethod]
    public async Task Throw_Exception_When_Try_Get_New_Access_Token_To_An_Unregistered_Refresh_Token()
    {
        string registeredUsername = "registered";
        var cache = CreateCache();
        string cacheEntry = JsonSerializer.Serialize(
            new UserAuthenticated() {
                Id = registeredUsername,
                RefreshToken = "registered_refresh_token",
                RefreshTokenExpiryTime = DateTime.MaxValue
            }
        );
        cache.SetString(registeredUsername, cacheEntry);
        var authService = CreateAuthServiceWithCache(cache);
        await Assert.ThrowsExceptionAsync<Exception>(() =>
            authService.NewAccessToken(
                registeredUsername, 
                "unregistered_refresh_token"
            )
        );
    }

    [TestMethod]
    public async Task Throw_Exception_When_Try_Get_New_Access_Token_With_An_Expired_Refresh_Token()
    {

        string username = "registered";
        string refreshToken = "registered_refresh_token";
        var cache = CreateCache();
        string cacheEntry = JsonSerializer.Serialize(
            new UserAuthenticated() {
                Id = username,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTime.MinValue
            }
        );
        cache.SetString(username, cacheEntry);
        var authService = CreateAuthServiceWithCache(cache);
        await Assert.ThrowsExceptionAsync<SecurityTokenExpiredException>(() =>
            authService.NewAccessToken(
                username, 
                refreshToken
            )
        );
    }

    [TestMethod]
    public async Task Return_New_Access_Token()
    {

        string username = "registered";
        string refreshToken = "registered_refresh_token";
        var cache = CreateCache();
        string cacheEntry = JsonSerializer.Serialize(
            new UserAuthenticated() {
                Id = username,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTime.MaxValue
            }
        );
        cache.SetString(username, cacheEntry);
        var authService = CreateAuthServiceWithCache(cache);
        string newToken = 
            await authService.NewAccessToken(username, refreshToken);
        JwtTokenCanBeRead(newToken);
    }

    void JwtTokenCanBeRead(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.IsTrue(tokenHandler.CanReadToken(token));
    }

    IDistributedCache CreateCache()
    {
        var opts = Options.Create<MemoryDistributedCacheOptions>(
            new MemoryDistributedCacheOptions()
        );
        return new MemoryDistributedCache(opts);
    }

    IAuthService CreateAuthServiceWithCache(IDistributedCache cache) {
        IGameHubState gameHubState = A.Fake<IGameHubState>();
        return new AuthService(
            FakeServerConfig(), 
            gameHubState, 
            cache);
    }

    IServerConfig FakeServerConfig() {
        byte[] secretKey = System.Text.Encoding.ASCII.GetBytes("someSecretKeyToUse");
        IServerConfig serverConfig = A.Fake<IServerConfig>();
        A.CallTo(() => serverConfig.SecretKey).Returns(secretKey);
        return serverConfig;
    }

    string StringContains(string token)
    {
        return A<string>.That.Matches(v => v.Contains(token));
    }
}