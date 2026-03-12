using UnityEngine;

public enum CardType
{
    Attack,
    Skill,
    Power
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare
}

public enum IntentionType
{
    Attack,
    Defend,
    Buff,
    Debuff,
    Unknown
}

[System.Serializable]
public class EnemyIntention
{
    public IntentionType type;
    public int value;
}