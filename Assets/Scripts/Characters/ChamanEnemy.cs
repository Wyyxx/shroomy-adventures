using UnityEngine;

public class ChamanEnemy : Enemy
{
    protected override void PrepareNextIntention()
    {
        currentIntention = new EnemyIntention { type = IntentionType.Attack, value = 6 };
    }
}