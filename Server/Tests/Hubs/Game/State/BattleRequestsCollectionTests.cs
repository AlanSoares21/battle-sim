using Microsoft.Extensions.Logging;
using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Tests.Hubs.Game.State;

[TestClass]
public class BattleRequestsCollectionTests
{
    [TestMethod]
    public void Add_Battle_Request() {
        BattleRequest requestToRegister = new();
        IBattleRequestCollection battleRequests = CreateBattleRequestCollection();
        bool result = battleRequests.TryAdd(requestToRegister);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Get_Battle_Request() {
        Guid requestId = Guid.NewGuid();
        BattleRequest requestToRegister = new() { requestId = requestId };
        IBattleRequestCollection battleRequests = CreateBattleRequestCollection();
        battleRequests.TryAdd(requestToRegister);
        BattleRequest requestRegistered = battleRequests.Get(requestId);
        Assert.AreEqual(requestToRegister, requestRegistered);
    }

    [TestMethod]
    public void When_Try_Add_Request_With_Duplicated_Id_Return_False() {
        Guid requestId = Guid.NewGuid();
        BattleRequest firstRequest = new() { requestId = requestId };
        BattleRequest secondRequest = new() { requestId = requestId };
        IBattleRequestCollection battleRequests = CreateBattleRequestCollection();
        battleRequests.TryAdd(firstRequest);
        bool secondAdditionResult = battleRequests.TryAdd(secondRequest);
        Assert.IsFalse(secondAdditionResult);
    }

    [TestMethod]
    public void Remove_Request() {
        Guid requestId = Guid.NewGuid();
        BattleRequest request = new() { requestId = requestId };
        IBattleRequestCollection battleRequests = CreateBattleRequestCollection();
        battleRequests.TryAdd(request);
        bool result = battleRequests.TryRemove(request);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Try_Get_A_Removed_Request_Throws_Exception() {
        Guid requestId = Guid.NewGuid();
        BattleRequest request = new() { requestId = requestId };
        IBattleRequestCollection battleRequests = CreateBattleRequestCollection();
        battleRequests.TryAdd(request);
        battleRequests.TryRemove(request);
        Assert.ThrowsException<KeyNotFoundException>(() => 
            battleRequests.Get(requestId));
    }

    [TestMethod]
    public void List_Requests_Where_User_Is_Target() {
        string userIdentifier = "someUserIdentifier";
        BattleRequest requestWithTheUserAsTarget = NewRequest();
        requestWithTheUserAsTarget.target = userIdentifier;
        IBattleRequestCollection battleRequests = CreateBattleRequestCollection();
        battleRequests.TryAdd(requestWithTheUserAsTarget);
        IList<BattleRequest> requestsWithUserAsTarget = battleRequests
            .RequestsWithUser(userIdentifier);
        Assert.AreEqual(
            requestWithTheUserAsTarget, 
            requestsWithUserAsTarget.First());
    }

    [TestMethod]
    public void List_Requests_Where_User_Is_Requester() {
        string userIdentifier = "someUserIdentifier";
        BattleRequest requestWithTheUserAsRequester = NewRequest();
        requestWithTheUserAsRequester.requester = userIdentifier;
        IBattleRequestCollection battleRequests = CreateBattleRequestCollection();
        battleRequests.TryAdd(requestWithTheUserAsRequester);
        IList<BattleRequest> requestsWithUserAsRequester = battleRequests
            .RequestsWithUser(userIdentifier);
        Assert.AreEqual(
            requestWithTheUserAsRequester, 
            requestsWithUserAsRequester.First());
    }

    [TestMethod]
    public void Dont_List_Requests_Without_This_User() {
        string userIdentifier = "someUserIdentifier";
        BattleRequest requestWithTheUserAsRequester = NewRequest();
        requestWithTheUserAsRequester.requester = userIdentifier;
        BattleRequest requestWithTheUserAsTarget = NewRequest();
        requestWithTheUserAsTarget.target = userIdentifier;
        BattleRequest requestWithoutTheUser = NewRequest();
        IBattleRequestCollection battleRequests = CreateBattleRequestCollection();
        battleRequests.TryAdd(requestWithoutTheUser);
        battleRequests.TryAdd(requestWithTheUserAsRequester);
        battleRequests.TryAdd(requestWithTheUserAsTarget);
        int ammountRequestsWithUser = 2;
        IList<BattleRequest> requestsWithUser = battleRequests
            .RequestsWithUser(userIdentifier);
        Assert.AreEqual(
            ammountRequestsWithUser, 
            requestsWithUser.Count);
    }

    BattleRequest NewRequest() => new() { requestId = Guid.NewGuid() };

    IBattleRequestCollection CreateBattleRequestCollection() => 
        new BattleRequestCollection(FakeLogger());

    ILogger<BattleRequestCollection> FakeLogger() => 
        A.Fake<ILogger<BattleRequestCollection>>();
}