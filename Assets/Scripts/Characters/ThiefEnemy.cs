using UnityEngine;

public class ThiefEnemy : Enemy
{
    protected override void PrepareNextIntention()
    {
        // En el futuro aquí pondrás: if (turno == 1) robar oro...
        // Por ahora, solo daño base:
        currentIntention = new EnemyIntention { type = IntentionType.Attack, value = 4 };
    }
}