using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class Detection : MonoBehaviour
{

    private Enemy enemy;
    public Vector3 idleDetectionRange;
    private Vector3 attackDetectionRange;
    [HideInInspector] public BoxCollider boxCollider;
    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = idleDetectionRange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enemy.attackList.Contains(other.transform) && !enemy.isDead && other.CompareTag("Player"))
        {
            enemy.attackList.Add(other.transform);
            boxCollider.size = idleDetectionRange * 4f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        enemy.attackList.Remove(other.transform);
        boxCollider.size = idleDetectionRange;
    }
}

