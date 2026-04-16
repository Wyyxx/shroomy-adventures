using UnityEngine;

public class LizardEnemy : Enemy
{
    protected override void PrepareNextIntention()
    {
        currentIntention = new EnemyIntention { type = IntentionType.Attack, value = 5 };
    }
}