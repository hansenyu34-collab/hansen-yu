using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [SerializeField] GameObject targetDoor;
    [SerializeField] AudioSource doorOpen;
    bool hasOpened = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasOpened && other.CompareTag("Player"))
        {
            doorOpen.Play();
            targetDoor.GetComponent<Animator>().Play("DoorOpen");
            hasOpened = true;
        }
    }
}
