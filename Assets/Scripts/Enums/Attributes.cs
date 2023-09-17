using System.ComponentModel;

namespace RPGTest.Enums
{
    public enum Attribute
    {
        None,
        //stats
        HP,
        [Description("Max HP")]
        MaxHP,
        MP,
        [Description("Max MP")]
        MaxMP,
        Stamina,
        [Description("Max STM")]
        MaxStamina,
        //Base stats
        BaseAttack,
        BaseDefense,
        BaseMagic,
        BaseResistance,
        [Description("ATK")]
        Attack,
        [Description("DEF")]
        Defense,
        [Description("MAG")]
        Magic,
        [Description("RES")]
        Resistance,

        HPPercentage,
        MPPercentage,
        StaminaPercentage,

        [Description("ATK")]
        TotalAttack,
        [Description("DEF")]
        TotalDefense,
        [Description("MAG")]
        TotalMagic,
        [Description("RES")]
        TotalResistance,
        TotalSpeed,
        TotalBlock,
        TotalAccuracy,

        [Description("SPD")]
        Speed,
        [Description("BLC")]
        Block,
        [Description("ACC")]
        Accuracy,

        Hit,
        TotalHit
    }

    public enum Aptitude
    {

    }
}
