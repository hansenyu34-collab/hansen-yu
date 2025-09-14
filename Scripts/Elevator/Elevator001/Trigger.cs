using UnityEngine;

public class Trigger : MonoBehaviour
{
    public Platform elevator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            elevator.ActivateElevator();

        }
    }
}
