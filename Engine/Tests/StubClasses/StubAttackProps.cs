using BattleSimulator.Engine.Interfaces.CharactersAttributes;

namespace BattleSimulator.Engine.Tests.StubClasses;

public class StubAttackProps : IOffensiveAttributes
{
    public int Damage { get; set; }
}
