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

        [Description("SPD")]
        Speed,
        [Description("BLC")]
        Block,
        [Description("ACC")]
        Accuracy,
        Hit,

        EquipmentAttack,
        EquipmentDefense,
        EquipmentMagic,
        EquipmentResistance,
    }

    public enum Aptitude
    {

    }
}
