using BattleSimulator.Server.Hubs;

namespace BattleSimulator.Server.Tests.Hubs.Game.State;

[TestClass]
public class ConnectionMappingTests
{
    [TestMethod]
    public void Add_Connection() {
        string userId = "someUserIdentifier";
        string connectionId = "someConnectionId";
        IConnectionMapping connectionMapping = new ConnectionMapping();
        bool result = connectionMapping
            .TryAdd(userId, connectionId);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Get_ConnectionId_By_UserId() {
        string userId = "someUserIdentifier";
        string connectionId = "someConnectionId";
        IConnectionMapping connectionMapping = new ConnectionMapping();
        connectionMapping.TryAdd(userId, connectionId);
        string connectionIdStored = connectionMapping
            .GetConnectionId(userId);
        Assert.AreEqual(connectionId, connectionIdStored);
    }

    [TestMethod]
    public void When_Try_Get_A_Connection_Not_Registered_Throws_Exception() {
        string userIdNotRegsitered = "userIdNotRegistered";
        IConnectionMapping connectionMapping = new ConnectionMapping();
        Assert.ThrowsException<KeyNotFoundException>(() => 
            connectionMapping.GetConnectionId(userIdNotRegsitered));
    }

    [TestMethod]
    public void Remove_Connection() {
        string userId = "someUserIdentifier";
        string connectionId = "someConnectionId";
        IConnectionMapping connectionMapping = new ConnectionMapping();
        connectionMapping.TryAdd(userId, connectionId);
        bool result = connectionMapping.TryRemove(userId, connectionId);;
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void When_Try_Get_A_Removed_Connection_Throws_Exception() {
        string userId = "someUserIdentifier";
        string connectionId = "someConnectionId";
        IConnectionMapping connectionMapping = new ConnectionMapping();
        connectionMapping.TryAdd(userId, connectionId);
        connectionMapping.TryRemove(userId, connectionId);;
        Assert.ThrowsException<KeyNotFoundException>(() => 
            connectionMapping.GetConnectionId(userId));
    }

    [TestMethod]
    public void List_Users_Ids_Registered() {
        string userId = "someUserIdentifier";
        string connectionId = "someConnectionId";
        IConnectionMapping connectionMapping = new ConnectionMapping();
        connectionMapping.TryAdd(userId, connectionId);
        var usersIds = connectionMapping.UsersIds();
        bool userIsInTheList = usersIds.Any(id => id == userId);
        Assert.IsTrue(userIsInTheList);
    }
}