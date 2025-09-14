using UnityEngine;

public class Respawn : MonoBehaviour
{
    public PlayerController player;
    public Transform RespawnPoint;

    public void RespawnToPosition()
    {
        player.transform.position = RespawnPoint.position;
        player.playerIsDead = false;
        Time.timeScale = 1f;
    }
}
