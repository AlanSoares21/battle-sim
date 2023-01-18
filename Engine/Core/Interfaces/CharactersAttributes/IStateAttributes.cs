namespace BattleSimulator.Engine.Interfaces.CharactersAttributes;

public interface IStateAttributes {
    int Health { get; set; }
    int HealthRegeneration { get; set; }
    int Energy { get; set; }
    int EnergyRegeneration { get; set; }
}