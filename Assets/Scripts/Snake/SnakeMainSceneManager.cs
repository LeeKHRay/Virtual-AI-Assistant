using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SnakeMainSceneManager : MonoBehaviour
{
    public AudioClip click;
    public AudioClip over;
    public GameObject mainMenu;
    public GameObject difficultyMenu;

    private AudioSource soundEffect;

    void Start()
    {
        soundEffect = GetComponent<AudioSource>();

        if (!SystemManager.Instance.prevIsFullscreen)
        {
            SystemManager.Instance.SetFullscreenMode();
        }
    }

    public void ShowDifficuly()
    {
        OnMouseClick();
        mainMenu.SetActive(false);
        difficultyMenu.SetActive(true);
    }

    public void ExitGame()
    {
        OnMouseClick();
        SceneManager.LoadScene("Main");
    }

    public void Back()
    {
        OnMouseClick();
        mainMenu.SetActive(true);
        difficultyMenu.SetActive(false);
    }

    public void StartGame(bool isEasy)
    {
        OnMouseClick();
        if (isEasy)
            SceneManager.LoadScene("SnakeEasy");
        else
            SceneManager.LoadScene("SnakeDifficult");
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
