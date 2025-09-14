using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    public GameObject pauseMenu;
    public GameObject settingMenu;
    public GameObject DeadMenu;
    public GameObject playerCanvas;
    private bool isPaused;
    private PlayerController player;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) return;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        isPaused = Time.timeScale == 0;

        if (!player.playerIsDead)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isPaused)
                {
                    Time.timeScale = 0;
                    pauseMenu.SetActive(true);
                }
                else
                {
                    Time.timeScale = 1;
                    pauseMenu.SetActive(false);
                    settingMenu.SetActive(false);
                    playerCanvas.SetActive(true);
                }
            }

            DeadMenu.SetActive(false);
        }
        else
        {
            DeadMenu.SetActive(true);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Continue()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }

    public void Respawn()
    {
        player.Respawn();
    }
}
