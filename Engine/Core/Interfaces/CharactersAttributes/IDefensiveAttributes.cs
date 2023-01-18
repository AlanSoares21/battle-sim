
namespace BattleSimulator.Engine.Interfaces.CharactersAttributes
{
    public interface IDefensiveAttributes
    {
        double PhysicalDefenseAbsorption { get; set; }
        double MagicalDefenseAbsorption { get; set; }
        double Dodge { get; set; }
        double Parry { get; set; }
        double Block { get; set; }
        double StealHealth { get; set; }
        double DamageReflection { get; set; }
    }
}