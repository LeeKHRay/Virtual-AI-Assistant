using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TetrisSceneManager : MonoBehaviour
{
    public enum State
    {
        Normal, Win, Lose, Draw, End
    }
    public static State state = State.Normal;

    public AudioClip click;
    public AudioClip over;
    public AudioClip win;
    public AudioClip lose;

    public GameObject pauseMenu;
    public GameObject gameEndMenu;
    public TMP_Text gameEndMessage;
    public TMP_Text playerScoreText;
    public TMP_Text aiScoreText;
    public TMP_Text timer;

    public TetrisGameboard playerGameboard;
    public TetrisGameboard aiGameboard;
    public static int playerScore = 0;
    public static int aiScore = 0;
    public bool isBattle = false;

    private bool isPause = false;
    private AudioSource soundEffect1;
    private AudioSource soundEffect2;
    private AudioSource bgm;
    private float time = 99.0f;
    private bool timesUp = false; 

    void Start()
    {
        Time.timeScale = 1.0f;
        state = State.Normal;

        playerScore = 0;
        aiScore = 0;
        if (playerScoreText != null)
        {
            playerScoreText.text = "0";
        }
        if (aiScoreText != null)
        {
            aiScoreText.text = "0";
        }

        bgm = GetComponents<AudioSource>()[0];
        soundEffect1 = GetComponents<AudioSource>()[1];
        soundEffect2 = GetComponents<AudioSource>()[2];
    }

    void Update()
    {
        if (playerScoreText != null && aiScoreText != null)
        {
            playerScoreText.text = playerScore + "";
            aiScoreText.text = aiScore + "";
        }

        if (Input.GetKeyDown(KeyCode.Escape) && state == State.Normal)
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

        // count-down timer
        if (timer != null && !timesUp && state == State.Normal)
        {
            time -= Time.deltaTime;
            if (time < 0)
            {
                time = 0.0f;
                timesUp = true;
            }
            int seconds = (int) Mathf.Ceil(time);
            timer.text = seconds < 10 ? "0" + seconds : "" + seconds;
        }

        if (state == State.Normal)
        {
            if ((playerGameboard.lose && aiGameboard.lose) || (timesUp && playerScore == aiScore))
            {
                state = State.Draw;
                playerGameboard.lose = false;
                aiGameboard.lose = false;
            }
            else if (aiGameboard.lose || (timesUp && playerScore > aiScore))
            {
                state = State.Win;
                aiGameboard.lose = false;
            }
            else if (playerGameboard.lose || (timesUp && playerScore < aiScore))
            {
                state = State.Lose;
                playerGameboard.lose = false;
            }
        }
        
        if (state == State.Win || state == State.Lose || state == State.Draw)
        {
            EndGame();
        }
    }

    // show game result
    private void EndGame()
    {
        State gameResult = state;
        state = State.End;

        if (gameResult == State.Win)
        {
            SoundEffect("win");
        }
        else
        {
            SoundEffect("lose");
        }
        bgm.Stop();

        gameEndMenu.SetActive(true);
        if (gameResult == State.Win)
        {
            gameEndMessage.text = "You Win";
        }
        else if (gameResult == State.Lose)
        {
            gameEndMessage.text = "<color=#1917E7>You Lose</color>";
        }
        else
        {
            gameEndMessage.text = "<color=#6AD156>Draw</color>";
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Tetris");
    }
    
    public void ResetGame()
    {
        bgm.Play();
        playerGameboard.Reset();
        aiGameboard.Reset();
        if (!isBattle)
        {
            time = 99.0f;
            timesUp = false;
            playerScore = 0;
            aiScore = 0;
            playerScoreText.text = "0";
            aiScoreText.text = "0";
        }
        gameEndMenu.SetActive(false);
        state = State.Normal;
        playerGameboard.spawner.Spawn();
        aiGameboard.spawner.Spawn();
    }

    public void Pause()
    {
        bgm.Pause();
        pauseMenu.SetActive(true);
        isPause = true;
        Time.timeScale = 0.0f;
    }

    public void Resume()
    {
        bgm.Play();
        pauseMenu.SetActive(false);
        isPause = false;
        Time.timeScale = 1.0f;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("TetrisMainMenu");
        Time.timeScale = 1.0f;
    }

    public void OnMouseClick()
    {
        soundEffect1.clip = click;
        soundEffect1.Play();
    }

    public void OnMouseOver()
    {
        soundEffect1.clip = over;
        soundEffect1.Play();
    }
	
    public void SoundEffect(string soundEffectName)
    {
        switch (soundEffectName)
        {
            case "win":
                soundEffect2.clip = win;
                break;
            case "lose":
                soundEffect2.clip = lose;
                break;
        }
        soundEffect2.Play();
    }
}
