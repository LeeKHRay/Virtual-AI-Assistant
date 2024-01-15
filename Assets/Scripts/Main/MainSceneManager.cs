using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Syrus.Plugins.DFV2Client;

public class MainSceneManager : MonoBehaviour
{
    public GameObject assistant;

    private Animator animator;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        animator = assistant.GetComponent<Animator>();
        audioSource = assistant.GetComponent<AudioSource>();

        SystemManager.Instance.SetReference();

        if (!SystemManager.Instance.prevIsFullscreen)
        {
            SystemManager.Instance.SetDesktopMode();
        }
    }

    public void PlayGame(DF2Response response)
    {
        string game = response.queryResult.parameters["game"].ToString();
        if (!string.IsNullOrEmpty(game))
        {
            if (game.Equals("Snake"))
            {
                SceneManager.LoadScene("SnakeMainMenu");
            }
            if (game.Equals("Tetris"))
            {
                SceneManager.LoadScene("TetrisMainMenu");
            }
        }
    }

    public void Quit()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        animator.SetTrigger("Quit"); // play “Farewell” animation
        StartCoroutine(WaitForQuit());
    }

    public IEnumerator WaitForQuit()
    {
        // wait for assistant finishes speech
        while (audioSource.isPlaying)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1);

        Application.Quit(); // close application
    }
}
