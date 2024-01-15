using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingSnakeGameboard : MonoBehaviour
{
    public AudioClip eat;
    public AudioClip move;
    public AudioClip lose;

    public static int width = 21;
    public static int height = 21;
    public Vector3 posOffset;
    public SnakeAgent playerSnake;
    public SnakeAgent aiSnake;
    public GameObject foodPrefab;

    public bool reset = false;
    public int bothReset = 0;

    private int eatFoodNum = 0;
    private int maxFoodNum = 3;
    private List<GameObject> foodObj;
    public List<Vector2> foodPos;

    void Start()
    {
        foodObj = new List<GameObject>();
        foodPos = new List<Vector2>();
        posOffset = transform.position;
        SpawnFood();
    }

    void Update()
    {
        if (reset)
        {
            playerSnake.Reset();
            aiSnake.Reset();
            ResetFood();
            SpawnFood();

            reset = false;
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
            if(eatFoodNum % 25 == 0) // increment maximum number of apples spawned on gameboard for every 25 eaten apples
            {
                maxFoodNum++;
            }
            return true;
        }
        return false;
    }

    public bool IsOutsideBoard(Vector2 pos)
    {
        return (int)pos.x < 0 || (int)pos.x >= width || (int)pos.y < 0 || (int)pos.y >= height;
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
}
