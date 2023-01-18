using System;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine;

namespace BattleSimulator.Engine;

public class Calculator : ICalculator
{
    public int Damage(int damage, double defenseAbsorption, bool autoAttack, AttackTarget target, IOffensiveAttributes attacker, IDefensiveAttributes defender)
    {
        // reduzindo absorção de dano pela penetração
        double damageAbsorption = attacker.Penetration >= defenseAbsorption ? 0 : defenseAbsorption - attacker.Penetration;
        // calculando dano
        double damageTaken = damage  - (damage * damageAbsorption);
        return (int)Math.Round(damageTaken);
    }
}
