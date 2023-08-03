
using BattleSimulator.Engine.Interfaces;

namespace BattleSimulator.Engine.Tests.Builders;

public class DuelBuilder
{
    IBoard? _board;
    IEventsObserver? _notifier;
    public DuelBuilder WithBoard(IBoard value) 
    {
        _board = value;
        return this;
    }
    
    public DuelBuilder WithEventObserver(IEventsObserver value) 
    {
        _notifier = value;
        return this;
    }
    
    public Duel Build() 
    {
        if (_board is null)
            _board = A.Fake<IBoard>();
        if (_notifier is null)
            _notifier = A.Fake<IEventsObserver>();

        return new Duel(
            System.Guid.NewGuid(),
            _board,
            new Calculator(),
            _notifier
        );
    }
}