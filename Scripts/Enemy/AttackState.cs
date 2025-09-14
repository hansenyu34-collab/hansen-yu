using UnityEngine;

public class AttackState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        enemy.animState = 2;
        enemy.targetPoint = enemy.attackList[0];
    }

    public override void OnUpdate(Enemy enemy)
    {   
        if (enemy.attackList.Count == 0)
        {
            enemy.TransitionToState(enemy.patrolState);
            return;
        }

        if (enemy.attackList.Count > 1)
        {
            for (int i = 1; i < enemy.attackList.Count; i++)
            {
                float curDist = Vector3.Distance(enemy.transform.position, enemy.attackList[i].position);
                float bestDist = Vector3.Distance(enemy.transform.position, enemy.targetPoint.position);

                if (curDist < bestDist)
                {
                    enemy.targetPoint = enemy.attackList[i];
                }
            }
        }


        if (enemy.attackList.Count == 1)
        {
            enemy.targetPoint = enemy.attackList[0];
        }

        if (enemy.targetPoint.tag == "Player")
        {
            enemy.AttackAction();
        }

        if (Vector3.Distance(enemy.transform.position, enemy.targetPoint.position) >= enemy.attackRange)
        {
            if(enemy.enemyType == Enemy.EnemyType.Paladin)
            {
                enemy.agent.speed = enemy.attackMovementSpeed;
                enemy.agent.acceleration = enemy.attackAcceleration;
                enemy.MoveToTarget();
            }          
        }
    }
}
