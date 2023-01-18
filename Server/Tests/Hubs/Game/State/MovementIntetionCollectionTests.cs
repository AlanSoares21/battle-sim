using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Models;

namespace BattleSimulator.Server.Tests.Hubs.Game.State;

[TestClass]
public class MovementIntetionCollectionTests
{
    [TestMethod]
    public void Add_Intention() {
        MovementIntention movement = new() {
            entityIdentifier = "entityId",
            cell = new(1, 2)
        };
        IMovementIntentionCollection collection = 
            new MovementIntentionCollection();
        var result = collection.TryAdd(movement);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Get_Intention() {
        MovementIntention movement = new() {
            entityIdentifier = "entityId",
            cell = new(1, 2)
        };
        IMovementIntentionCollection collection = 
            new MovementIntentionCollection();
        collection.TryAdd(movement);
        var movementRegistered = 
            collection.Get(movement.entityIdentifier);
        Assert.AreEqual(movementRegistered, movement);
    }

    [TestMethod]
    public void Throws_Excepion_When_Try_Get_Intention_Not_Registered() {
        IMovementIntentionCollection collection = 
            new MovementIntentionCollection();
        Assert.ThrowsException<KeyNotFoundException>(
            () => collection.Get("notRegisteredEntityId")
        );
    }

    [TestMethod]
    public void List_All_Intentions_Added() {
        MovementIntention movementA, movementB;
        movementA = new() { 
            entityIdentifier = "entityA"
        };
        movementB = new() {
            entityIdentifier = "entityB"
        };
        IMovementIntentionCollection collection = 
            new MovementIntentionCollection();
        collection.TryAdd(movementA);
        collection.TryAdd(movementB);
        var movements = collection.List();
        Assert.IsTrue(movements.Contains(movementA));
        Assert.IsTrue(movements.Contains(movementB));
    }

    [TestMethod]
    public void Overwrite_Old_Intention_When_Try_Add_A_New_For_The_Same_Entity() 
    {
        string entityId = "entityId";
        MovementIntention oldIntention, newIntention;
        oldIntention = new() { 
            entityIdentifier = entityId
        };
        newIntention = new() {
            entityIdentifier = entityId
        };
        IMovementIntentionCollection collection = 
            new MovementIntentionCollection();
        collection.TryAdd(oldIntention);
        collection.TryAdd(newIntention);
        var movements = collection.List();
        Assert.IsFalse(movements.Contains(oldIntention));
        Assert.IsTrue(movements.Contains(newIntention));
    }

    [TestMethod]
    public void Remove_Intention() {
        MovementIntention intention = new() { 
            entityIdentifier = "entityId"
        };
        IMovementIntentionCollection collection = 
            new MovementIntentionCollection();
        collection.TryAdd(intention);
        bool result = collection.TryRemove(intention);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Do_Not_List_Removed_Intentions() {
        MovementIntention intentionToStore = new() { 
            entityIdentifier = "entityA"
        };
        MovementIntention intentionToRemove = new() { 
            entityIdentifier = "entityB"
        };
        IMovementIntentionCollection collection = 
            new MovementIntentionCollection();
        collection.TryAdd(intentionToStore);
        collection.TryAdd(intentionToRemove);
        collection.TryRemove(intentionToRemove);
        var movements = collection.List();
        Assert.IsTrue(movements.Contains(intentionToStore));
        Assert.IsFalse(movements.Contains(intentionToRemove));
    }

    [TestMethod]
    public void Throws_Exception_When_Try_Get_A_Removed_Intention() {
        MovementIntention intentionToRemove = new() { 
            entityIdentifier = "entityB"
        };
        IMovementIntentionCollection collection = 
            new MovementIntentionCollection();
        collection.TryAdd(intentionToRemove);
        collection.TryRemove(intentionToRemove);
        Assert.ThrowsException<KeyNotFoundException>(
            () => collection.Get(intentionToRemove.entityIdentifier));
    }
}