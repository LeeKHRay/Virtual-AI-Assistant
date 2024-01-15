using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TetrisMainSceneManager : MonoBehaviour
{
    public AudioClip click;
    public AudioClip over;
    public GameObject mainMenu;
    public GameObject modeMenu;

    private AudioSource soundEffect;

    void Start()
    {
        soundEffect = GetComponent<AudioSource>();

        if (!SystemManager.Instance.prevIsFullscreen)
        {
            SystemManager.Instance.SetFullscreenMode();
        }
    }

    public void ShowModes()
    {
        mainMenu.SetActive(false);
        modeMenu.SetActive(true);
    }

    public void StartGame(bool isTimeMode)
    {
        if (isTimeMode)
            SceneManager.LoadScene("TetrisTime");
        else
            SceneManager.LoadScene("TetrisBattle");
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void Back()
    {
        mainMenu.SetActive(true);
        modeMenu.SetActive(false);
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
