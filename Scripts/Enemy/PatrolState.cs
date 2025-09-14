using System;
using UnityEngine;

public class PatrolState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        if (enemy.canPatrol)
        {
            enemy.LoadPath(enemy.wayPointObject[0]);
            enemy.animState = 0;
        }
    }

    public override void OnUpdate(Enemy enemy)
    {
        if (enemy.canPatrol)
        {
            if (enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                enemy.animState = 1;
                enemy.agent.isStopped = false;
                enemy.MoveToTarget();
                enemy.agent.speed = enemy.patrolMovementSpeed;
                enemy.agent.acceleration = enemy.patrolAcceleration;
            }
        }

        if (enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            enemy.animState = 0;
            enemy.agent.isStopped = true;
            enemy.agent.velocity = Vector3.zero;
            enemy.agent.ResetPath();
        }


        float distance = Vector3.Distance(enemy.transform.position, enemy.wayPoints[enemy.index]);

        if (distance <= 1.5f)
        {
            enemy.animState = 0;

            enemy.index++;
            if (enemy.index == enemy.wayPoints.Count)
            {
                enemy.index = 0;
            }
        }

        if (enemy.attackList.Count > 0)
        {
            enemy.TransitionToState(enemy.attackState);
        }
    }
}

