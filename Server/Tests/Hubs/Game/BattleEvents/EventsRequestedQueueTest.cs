using BattleSimulator.Server.Hubs;
using BattleSimulator.Server.Hubs.EventHandling;

namespace BattleSimulator.Server.Tests.Hubs.Game.BattleEvents;

[TestClass]
public class EventsRequestedQueueTest
{
    [TestMethod]
    public void Enqueue_Event() 
    {
        string source = "source";
        string target = "target";
        IGameEvent gameEvent = NewGameEvent(source, target);
        IEventsQueue eventsQueue = NewQueue();
        Assert.IsTrue(eventsQueue.IsEmpty());
        eventsQueue.Enqueue(gameEvent);
        Assert.IsFalse(eventsQueue.IsEmpty());
    }

    [TestMethod]
    public void Dequeue_Event() 
    {
        string source = "source";
        string target = "target";
        IGameEvent gameEvent = NewGameEvent(source, target);
        var queue = new EventsQueue();
        queue.Enqueue(gameEvent);
        var dequeuedEvent = queue.Dequeue();
        Assert.AreEqual(gameEvent, dequeuedEvent);
        Assert.IsTrue(queue.IsEmpty());
    }

    IGameEvent NewGameEvent(string source, string target) {
        var value = A.Fake<IGameEvent>();
        A.CallTo(() => value.Source).Returns(source);
        A.CallTo(() => value.Target).Returns(target);
        return value;
    }

    IEventsQueue NewQueue() => new EventsQueue();
}