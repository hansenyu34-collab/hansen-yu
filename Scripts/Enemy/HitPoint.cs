using UnityEngine;

public class HitPoint : MonoBehaviour
{
    private Enemy enemy;
    private PlayerController player;
    public bool isEnemy;

    public void OnTriggerEnter(Collider other)
    {

        enemy = GetComponentInParent<Enemy>();
            
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().PlayerHealth(Random.Range(enemy.minDamage, enemy.maxDamage));
        }
    }
}

