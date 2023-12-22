using RPGTest.Models;
using System.Collections.Generic;

public class SkillNode : IdObject
{    
    public string Description { get; set; }

    public int MaxRank { get; set; }

    public List<SkillNodeUnlock> Abilities { get; set; }

    public List<SkillNodeUnlock> Effects { get; set; }
}

public class SkillNodeUnlock : IdObject
{
    public int MaxRank { get; set; } = -1;

    public int RequiredRank { get; set; } = -1;
}