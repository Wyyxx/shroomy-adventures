using UnityEngine;

public class SpiderEnemy : Enemy
{
    protected override void PrepareNextIntention()
    {
        currentIntention = new EnemyIntention { type = IntentionType.Attack, value = 7 };
    }
}