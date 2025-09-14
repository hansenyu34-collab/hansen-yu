using UnityEngine;

public abstract class EnemyBaseState
{
    public abstract void EnemyState(Enemy enemy);

    public abstract void OnUpdate(Enemy enemy);
}
