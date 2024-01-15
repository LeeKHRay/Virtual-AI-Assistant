using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SnakeSceneManager : MonoBehaviour
{
    public AudioClip click;
    public AudioClip over;
    public GameObject pauseMenu;
    public SnakeGameboard gameboard;
    public static AudioSource bgm;

    private bool isPause = false;
    private AudioSource soundEffect;

    void Start()
    {
        Time.timeScale = 1.0f;
        bgm = GetComponents<AudioSource>()[0];
        soundEffect = GetComponents<AudioSource>()[1];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameboard.state == SnakeGameboard.State.Normal)
        {
            if (isPause)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void StartGame(bool isEasy)
    {
        OnMouseClick();
        if (isEasy)
            SceneManager.LoadScene("SnakeEasy");
        else
            SceneManager.LoadScene("SnakeDifficult");
    }

    public void ResetGame()
    {
        OnMouseClick();
        bgm.Play();
        gameboard.ResetGame();
    }

    public void Pause()
    {
        OnMouseClick();
        bgm.Pause();
        pauseMenu.SetActive(true);
        isPause = true;
        Time.timeScale = 0.0f;
    }

    public void Resume()
    {
        OnMouseClick();
        bgm.Play();
        pauseMenu.SetActive(false);
        isPause = false;
        Time.timeScale = 1.0f;
    }

    public void MainMenu()
    {
        OnMouseClick();
        SceneManager.LoadScene("SnakeMainMenu");
        Time.timeScale = 1.0f;
    }

    public void OnMouseClick()
    {
        soundEffect.clip = click;
        soundEffect.Play();
    }

    public void OnMouseOver()
    {
        soundEffect.clip = over;
        soundEffect.Play();
    }
}
