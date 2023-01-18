using System.IdentityModel.Tokens.Jwt;
using BattleSimulator.Server.Auth;
using BattleSimulator.Server.Hubs;

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
        return new AuthService(serverConfig, gameHubState);
    }

    [TestMethod]
    public void The_Server_Config_Is_Being_Used() {
        const string username = "username";
        IServerConfig serverConfig = FakeServerConfig();
        var authService = CreateAuthServiceWithThisServerConfig(serverConfig);
        authService.GenerateJwtToken(username);
        A.CallTo(() => serverConfig.Audience).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.ClaimTypeName).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.Issuer).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.SecondsAuthTokenExpire).MustHaveHappenedOnceOrMore();
        A.CallTo(() => serverConfig.SecretKey).MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public void Generate_Valid_JWT_Token() {
        const string username = "username";
        IServerConfig serverConfig = FakeServerConfig();
        var authService = CreateAuthServiceWithThisServerConfig(serverConfig);
        string jwtToken= authService.GenerateJwtToken(username);
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.IsTrue(tokenHandler.CanReadToken(jwtToken));
    }

    [TestMethod]
    public void Generate_JWT_Token_With_The_ClaimIndentity_Of_Server_Config() {
        const string username = "username";
        IServerConfig serverConfig = FakeServerConfig();
        string claimTypeName = "SomeValuToClaimTypeName";
        A.CallTo(() => serverConfig.ClaimTypeName).Returns(claimTypeName);
        var authService = CreateAuthServiceWithThisServerConfig(serverConfig);
        string jwtToken= authService.GenerateJwtToken(username);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtTokenData = tokenHandler.ReadJwtToken(jwtToken);
        Assert.IsTrue(
            jwtTokenData.Claims
                .Any(claim => claim.Type == claimTypeName)
        );
    }

    public IAuthService CreateAuthServiceWithThisServerConfig(IServerConfig serverConfig) {
        IGameHubState gameHubState = A.Fake<IGameHubState>();
        return new AuthService(serverConfig, gameHubState);
    }

    public IServerConfig FakeServerConfig() {
        byte[] secretKey = System.Text.Encoding.ASCII.GetBytes("someSecretKeyToUse");
        IServerConfig serverConfig = A.Fake<IServerConfig>();
        A.CallTo(() => serverConfig.SecretKey).Returns(secretKey);
        return serverConfig;
    }
}