using System.ComponentModel;

namespace RPGTest.Enums
{
    public enum Attribute
    {
        None,
        //stats
        CurrentHP,
        [Description("Max HP")]
        HP,
        CurrentMP,
        [Description("Max MP")]
        MP,
        CurrentStamina,
        [Description("Max STM")]
        Stamina,
        //Base stats
        TotalStrenght,
        [Description("STR")]
        Strength,
        TotalConstitution,
        [Description("CST")]
        Constitution,
        TotalDexterity,
        [Description("DXT")]
        Dexterity,
        TotalAgility,
        [Description("AGI")]
        Agility,
        TotalMental,
        [Description("MTN")]
        Mental,
        TotalResilience,
        [Description("RES")]
        Resilience,
        [Description("SPD")]
        Speed,
        [Description("BLK")]
        Block,
        [Description("ACC")]
        Accuracy
    }

    public enum Aptitude
    {

    }
}
