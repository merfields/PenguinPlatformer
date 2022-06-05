using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private bool gameIsPaused = false;

    [SerializeField] GameObject pauseMenuUi;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        pauseMenuUi.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }
    public void Resume()
    {
        pauseMenuUi.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;

    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
