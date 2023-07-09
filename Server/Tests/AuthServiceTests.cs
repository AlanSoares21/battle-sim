using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using BattleSimulator.Server.Auth;
using BattleSimulator.Server.Hubs;
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
    public void The_Server_Config_Is_Being_Used_To_Create_Access_Tokens() 
    {
        const string username = "username";
        IServerConfig serverConfig = FakeServerConfig();
        var authService = CreateAuthServiceWithThisServerConfig(serverConfig);
        authService.CreateAccessToken(username);
        A.CallTo(() => serverConfig.Audience).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.ClaimTypeName).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.Issuer).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.SecondsAuthTokenExpire).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.SecretKey).MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public void Generate_Valid_JWT_Token() 
    {
        const string username = "username";
        IServerConfig serverConfig = FakeServerConfig();
        var authService = CreateAuthServiceWithThisServerConfig(serverConfig);
        var jwtToken = authService.CreateAccessToken(username);
        JwtTokenCanBeRead(jwtToken);
    }

    [TestMethod]
    public void Generate_JWT_Token_With_The_ClaimIndentity_Of_Server_Config() 
    {
        const string username = "username";
        IServerConfig serverConfig = FakeServerConfig();
        string claimTypeName = "SomeValuToClaimTypeName";
        A.CallTo(() => serverConfig.ClaimTypeName).Returns(claimTypeName);
        var authService = CreateAuthServiceWithThisServerConfig(serverConfig);
        var jwtToken = authService.CreateAccessToken(username);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtTokenData = tokenHandler.ReadJwtToken(jwtToken);
        Assert.IsTrue(
            jwtTokenData.Claims
                .Any(claim => claim.Type == claimTypeName)
        );
    }

    [TestMethod]
    public async Task Register_Refresh_Token_On_Cache()
    {
        const string username = "username";
        const string refreshToken = "some_refresh_token";
        var cache = CreateCache();
        var authService = CreateAuthServiceWithCache(cache);
        await authService.StoreRefreshToken(username, refreshToken);
        var jsonRegister = cache.GetString(username);
        Assert.IsNotNull(jsonRegister, "cache dont have a register with username as key");
        var register = JsonSerializer
            .Deserialize<UserAuthenticated>(jsonRegister);
        Assert.IsNotNull(register, $"The json information stored in the cache dont match the model. {jsonRegister}");
        Assert.IsNotNull(register.RefreshToken, "The refresh token stored in the cache is null");
        Assert.AreEqual(
            refreshToken, 
            register.RefreshToken, 
            $"The refresh token stored in the cahce is different than the token used. cache: {register.RefreshToken} - original token: {refreshToken}"
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
            authService.GetUserAuthenticated(unregisteredUsername)
        );
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

    [TestMethod]
    public void Throw_Exception_When_Try_Get_Username_From_Malformed_Access_Token()
    {
        string malFormedAccessToken = "malformed.access.token";
        var authService = CreateAuthServiceWithCache(A.Fake<IDistributedCache>());
        Assert.ThrowsException<ArgumentException>(() => 
            authService.GetUsernameFromAccessToken(malFormedAccessToken)
        );
    }

    [TestMethod]
    public void Return_Username_From_An_Access_Token()
    {
        string username = "someUsername";
        var serverConfig = FakeServerConfig();
        A.CallTo(() => serverConfig.Audience).Returns("audience");
        A.CallTo(() => serverConfig.Issuer).Returns("issuer");
        A.CallTo(() => serverConfig.ClaimTypeName).Returns(ClaimTypes.NameIdentifier);
        var authService = CreateAuthServiceWithThisServerConfig(serverConfig);
        string token  = authService.CreateAccessToken(username);
        var tokenUsername = authService.GetUsernameFromAccessToken(token);
        Assert.AreEqual(username, tokenUsername);
    }

    IAuthService CreateAuthServiceWithThisServerConfig(IServerConfig serverConfig) {
        IGameHubState gameHubState = A.Fake<IGameHubState>();
        return new AuthService(
            serverConfig, 
            gameHubState, 
            A.Fake<IDistributedCache>());
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
}