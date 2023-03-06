using System;
using BattleSimulator.Engine.Interfaces.CharactersAttributes;
using BattleSimulator.Engine.Interfaces;
using BattleSimulator.Engine;

namespace BattleSimulator.Engine;

public class Calculator : ICalculator
{
    public int Damage(
        int damage, 
        double defenseAbsorption, 
        IOffensiveAttributes attacker, 
        IDefensiveAttributes defender)
    {
        // calculando dano
        double damageTaken = damage  - (damage * defenseAbsorption);
        return (int)Math.Round(damageTaken);
    }
}
