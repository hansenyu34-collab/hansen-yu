using UnityEngine;

public class MagicAttack : MonoBehaviour
{
    public float sphereSpeed;
    public float minDamage;
    public float maxDamage;
    public bool canMove;
    public GameObject sphereAttackHit;
    private Rigidbody rb;
    void Awake()
    {
        if (canMove)
        {
            rb = GetComponent<Rigidbody>();

            Transform target = Camera.main.transform;
            Vector3 direction = target.position - transform.position;

            rb.AddForce(direction * sphereSpeed);
        }

        Destroy(gameObject, 3f);
    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        { 
            other.GetComponent<PlayerController>().PlayerHealth(Random.Range(minDamage, maxDamage));
           
            if (canMove)
            {
                Instantiate(sphereAttackHit, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
        else if (other.tag == "Environment" && canMove)
        {
            Instantiate(sphereAttackHit, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
