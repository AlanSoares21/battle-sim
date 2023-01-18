using BattleSimulator.Engine.Interfaces.CharactersAttributes;

namespace BattleSimulator.Engine.Interfaces;

public interface ICalculator {
    int Damage(int damage, double defenseAbsorption, bool autoAttack, AttackTarget target, IOffensiveAttributes attacker, IDefensiveAttributes defender);
}
