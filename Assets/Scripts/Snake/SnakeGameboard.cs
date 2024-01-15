using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SnakeGameboard : MonoBehaviour
{
    public enum State
    {
        Normal, Win, Lose, Draw, End
    }

    public AudioClip eat;
    public AudioClip move;
    public AudioClip win;
    public AudioClip lose;

    public TMP_Text playerSnakeLength;
    public TMP_Text aiSnakeLength;
    public GameObject gameEndMenu;
    public TMP_Text gameEndMessage;

    public static int width = 21;
    public static int height = 21;
    public Vector3 posOffset;
    public SnakePlayer playerSnake;
    public SnakeOpponent aiSnake;
    public GameObject foodPrefab;

    public State state = State.Normal;

    private int eatFoodNum = 0;
    private int maxFoodNum = 3;
    private List<GameObject> foodObj;
    public List<Vector2> foodPos;

    private AudioSource soundEffect;

    void Start()
    {
        foodObj = new List<GameObject>();
        foodPos = new List<Vector2>();
        posOffset = transform.position;
        soundEffect = GetComponent<AudioSource>();
        SpawnFood();
    }

    void Update()
    {
        playerSnakeLength.text = "Length: " + playerSnake.length.ToString();
        aiSnakeLength.text = "Length: " + aiSnake.length.ToString();
        if (playerSnake.lose && aiSnake.lose)
        {
            state = State.Draw;
            playerSnake.lose = false;
            aiSnake.lose = false;
        }

        if (state == State.Win || state == State.Lose || state == State.Draw)
        {
            EndGame();            
        }
    }

    public void SpawnFood()
    {
        int foodNum = foodObj.Count;
        for (int i = 0; i < maxFoodNum - foodNum; i++)
        {
            Vector2 tmpFoodPos;
            do
            {
                tmpFoodPos = new Vector2(Random.Range(0, width), Random.Range(0, height));
            } while (playerSnake.GetAllPos().IndexOf(tmpFoodPos) != -1
            || aiSnake.GetAllPos().IndexOf(tmpFoodPos) != -1 || foodPos.IndexOf(tmpFoodPos) != -1); // avoid spawning apples on snakes and other spawned apples

            foodPos.Add(tmpFoodPos);
            foodObj.Add(Instantiate(foodPrefab, tmpFoodPos + new Vector2(posOffset.x, posOffset.y), Quaternion.identity, transform));
        }
    }

    public bool TryEatFood(Vector2 snakePos)
    {
        int idx = foodPos.IndexOf(snakePos);
        if (foodPos.IndexOf(snakePos) != -1) // check if the snake eat an apple
        {
            Destroy(foodObj[idx]);
            foodObj.RemoveAt(idx);
            foodPos.RemoveAt(idx);

            eatFoodNum++;
            if (eatFoodNum % 25 == 0) // increment maximum number of apples spawned on gameboard for every 25 eaten apples
            {
                maxFoodNum++;
            }
            return true;
        }
        return false;
    }

    public bool IsOutsideBoard(Vector2 pos)
    {
        return ((int)pos.x < 0 || (int)pos.x >= width || (int)pos.y < 0 || (int)pos.y >= height);
    }

    public void ResetFood()
    {
        foreach (GameObject food in foodObj)
        {
            Destroy(food);
        }
        eatFoodNum = 0;
        maxFoodNum = 3;
        foodObj.Clear();
        foodPos.Clear();
    }

    public void ResetGame()
    {
        ResetFood();
        SpawnFood();
        playerSnake.Reset();
        aiSnake.Reset();
        gameEndMenu.SetActive(false);
        playerSnake.enabled = true;
        aiSnake.enabled = true;
        state = State.Normal;
    }

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
        SnakeSceneManager.bgm.Stop();

        playerSnake.enabled = false;
        aiSnake.enabled = false;

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

    public void SoundEffect(string soundEffectName)
    {
        switch (soundEffectName)
        {
            case "eat":
                soundEffect.clip = eat;
                break;
            case "move":
                soundEffect.clip = move;
                break;
            case "win":
                soundEffect.clip = win;
                break;
            case "lose":
                soundEffect.clip = lose;
                break;
        }
        soundEffect.Play();
    }
}
