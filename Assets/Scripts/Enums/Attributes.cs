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
        AttackP1,
        TotalAttackP1,
        AttackP2,
        TotalAttackP2,
        DefenseP1,
        TotalDefenseP1,
        DefenseP2,
        TotalDefenseP2,
        MagicP1,
        TotalMagicP1,
        MagicP2,
        TotalMagicP2,
        ResistanceP1,
        TotalResistanceP1,
        ResistanceP2,
        TotalResistanceP2,

        [Description("SPD")]
        Speed,
        [Description("BLC")]
        Block,
        [Description("ACC")]
        Accuracy
    }

    public enum Aptitude
    {

    }
}
