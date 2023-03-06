using BattleSimulator.Engine.Interfaces.CharactersAttributes;

namespace BattleSimulator.Engine.Interfaces;

public interface ICalculator {
    int Damage(int damage, double defenseAbsorption, IOffensiveAttributes attacker, IDefensiveAttributes defender);
}
