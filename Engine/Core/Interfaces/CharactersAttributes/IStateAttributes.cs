namespace BattleSimulator.Engine.Interfaces.CharactersAttributes;

public interface IStateAttributes {
    int HealthRadius { get; set; }
    Coordinate CurrentHealth { get; set; }
    int Mana { get; set; }
    int MaxMana { get; set; }
}