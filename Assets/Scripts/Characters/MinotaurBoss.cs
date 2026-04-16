using UnityEngine;

public class MinotaurBoss : Enemy
{
    protected override void PrepareNextIntention()
    {
        currentIntention = new EnemyIntention { type = IntentionType.Attack, value = 12 };
    }
}