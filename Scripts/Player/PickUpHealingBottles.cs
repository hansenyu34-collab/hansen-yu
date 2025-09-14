using UnityEngine;

public class PickUpHealingBottles : MonoBehaviour
{
    private PlayerController playerController;
    private AudioSource audioSource;
    public AudioClip pickUpSound;
    private float rotationSpeed;

    void Start()
    {
        rotationSpeed = 100f;
        audioSource = GetComponent<AudioSource>();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        transform.eulerAngles += new Vector3(0, rotationSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Invoke("DestroyItem", 0.3f);
            playerController. bottleAmount++;
            audioSource.PlayOneShot(pickUpSound);
        }
    }
    
    private void DestroyItem()
    {
        Destroy(gameObject);
    }
}

