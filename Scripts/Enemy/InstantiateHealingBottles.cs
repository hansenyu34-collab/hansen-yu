using UnityEngine;

public class InstantiateHealingBottles : MonoBehaviour
{
    public GameObject healingBottles;

    public bool hasInstantiated;

    private Enemy enemy;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        hasInstantiated = false;
    }
    private void Update()
    {
        if (!hasInstantiated)
        {
            if (enemy.isDead)
            {
                Instantiate(healingBottles, transform.position + new Vector3(0, 3, 0), Quaternion.identity);

                hasInstantiated = true;
            }
        }
    }
}
