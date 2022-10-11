
namespace RPGTest.Enums
{
    public enum TargetType
    {
        None,
        Self,
        SingleAlly,
        RandomAlly,
        Allies,
        SingleEnemy,
        RandomEnemy,
        Enemies,
        All,
        Global
    }

    public enum EffectType
    {
        Heal,
        Damage,
        Buff,
        Debuff,
        Alter,
        Cleanse
    }

    public enum DamageType
    {
        HP,
        Mana,
        Stamina
    }
}
