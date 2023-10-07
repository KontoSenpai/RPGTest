using System;

[AttributeUsage(AttributeTargets.All)]
public class NameAttribute : Attribute
{
    public static readonly NameAttribute Default;

    public NameAttribute() { }
    public NameAttribute(string name)
    {
        this.Name = name;
    }

    public virtual string Name { get; }
    public override bool IsDefaultAttribute() { return false; }
}

[AttributeUsage(AttributeTargets.All)]
public class ShortNameAttribute : Attribute
{
    public static readonly ShortNameAttribute Default;

    public ShortNameAttribute() {}
    public ShortNameAttribute(string shortName)
    {
        this.ShortName = shortName;
    }

    public virtual string ShortName { get; }
    public override bool IsDefaultAttribute() { return false; }
}