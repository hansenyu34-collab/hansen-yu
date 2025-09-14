using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour
{
    [SerializeField] Transform targetPosition;
    [SerializeField] GameObject[] airWalls;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip trigger;
    [SerializeField] AudioClip moving;
    [SerializeField] float speed;
    [SerializeField] float delayTime;
    [SerializeField] float lockDuration = 3f;  

    private bool isMoving = false;
    private bool isLocked = false;
    private bool goingUp = false;
    private Vector3 initialPosition;

    private PlayerController player;

    void Start()
    {
        initialPosition = transform.position;
        foreach (GameObject walls in airWalls)
        {
            walls.SetActive(false);
        }

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    public void ActivateElevator()
    {
        if (isMoving || isLocked)
            return;

        StartCoroutine(StartElevatorAfterDelay());
    }

    private IEnumerator StartElevatorAfterDelay()
    {
        audioSource.PlayOneShot(trigger);
        yield return new WaitForSeconds(delayTime);

        foreach (GameObject walls in airWalls)
        {
            walls.SetActive(true);
        }

        isMoving = true;
        audioSource.clip = moving;
        audioSource.Play();
    }

    void Update()
    {
        if (isMoving)
        {
            MoveElevator();
        }

        if (player.playerIsDead)
        {
            transform.position = initialPosition;
            
            foreach(GameObject w in airWalls)
            {
                w.SetActive(false);
            }

            goingUp = false;
        }
    }

    private void MoveElevator()
    {
        Vector3 destination = goingUp ? initialPosition : targetPosition.position;
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            isMoving = false;
            audioSource.Stop();

            if (!goingUp)
            {
                airWalls[0].SetActive(false);
            }
            else
            {
                foreach (GameObject walls in airWalls)
                {
                    walls.SetActive(false);
                }
            }

            goingUp = !goingUp;

            StartCoroutine(LockElevator());
        }
    }

    private IEnumerator LockElevator()
    {
        isLocked = true;
        yield return new WaitForSeconds(lockDuration);
        isLocked = false;
    }
}
