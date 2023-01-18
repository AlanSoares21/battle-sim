using BattleSimulator.Engine.Interfaces.CharactersAttributes;

namespace BattleSimulator.Engine.Tests.StubClasses
{
    class StubDefenseProps : IDefensiveAttributes
    {
        public double PhysicalDefenseAbsorption { get; set; }
        public double MagicalDefenseAbsorption { get; set; }
        public double Dodge { get; set; }
        public double Parry { get; set; }
        public double Block { get; set; }
        public double StealHealth { get; set; }
        public double DamageReflection { get; set; }
    }
}