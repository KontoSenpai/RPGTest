
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
        Alter,
        Buff,
        Cleanse,
        Damage,
        Debuff,
        Heal,
        Passive,
        Special,
        Status
    }

    public enum DamageType
    {
        HP,
        Mana,
        Stamina
    }
}
