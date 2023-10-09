using System.ComponentModel;

namespace RPGTest.Enums
{
    public enum Attribute
    {
        None,
        //stats
        HP,
        [Name("Max HP")]
        [Description("Toto")]
        MaxHP,
        MP,
        [Name("Max MP")]
        [Description("Reserve of mana, resource used to cast magic")]
        MaxMP,
        Stamina,
        [Name("Max STM")]
        [Description("Reserve of stamina, resource used to perform physical abilities")]
        MaxStamina,
        [Name("Attack")]
        [ShortName("ATK")]
        [Description("Value used to calculate damage dealt by physical attacks")]
        Attack,
        [Name("Defense")]
        [ShortName("DEF")]
        [Description("Value used to calculate damage dealt by magical attacks")]
        Defense,
        [Name("Magic")]
        [ShortName("MAG")]
        [Description("Value used to reduce damage received by physical attacks")]
        Magic,
        [Name("Resistance")]
        [ShortName("RES")]
        [Description("Value used to reduce damage received by magical attacks")]
        Resistance,

        HPPercentage,
        MPPercentage,
        StaminaPercentage,

        [Name("Speed")]
        [ShortName("SPD")]
        [Description("Value used to determine how fast entity performs action")]
        Speed,
        [Name("Block")]
        [ShortName("BLK")]
        [Description("Chance to block incoming attack and reduce damage taken")]
        Block,
        [Name("Accuracy")]
        [ShortName("ACC")]
        [Description("Chance to hit target")]
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
