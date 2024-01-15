using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;
using System;

public class SnakeAgent : Agent
{
    public enum Direction
    {
        Up, Down, Left, Right
    }

    public GameObject bodyPrefab;
    public SnakeAgent opponentSnake;
    public Vector2 initPos;
    public Vector3 initRot;
    public Direction initDir;
    public bool isEasy = true;

    private Direction curDir;
    private Direction nextDir;
    private Vector2 pos;
    private Vector2 dirVec;
    private int targetFoodIdx;
    private float moveTimeInterval = 0.5f;
    private float timer = 0.0f;
    private TrainingSnakeGameboard gameboard;
    private int length = 2;
    public List<Vector2> posList;
    private List<Transform> bodyList;

    void Awake()
    {
        gameboard = transform.parent.GetComponent<TrainingSnakeGameboard>();
    }

    void Update()
    {
        if (!gameboard.reset)
        {
            timer += Time.deltaTime;
            if (timer >= moveTimeInterval)
            {
                timer -= moveTimeInterval;
                RequestDecision(); // take observations and perform actions
            }
        }
    }
    public override void Initialize()
    {
        Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float dist = 40.0f;
        if(!isEasy || (isEasy && targetFoodIdx == -1)) { // find out the nearest apple
            for (int i = 0; i < gameboard.foodPos.Count; i++)
            {
                float newDist = Vector2.Distance(pos, gameboard.foodPos[i]);
                if (newDist < dist)
                {
                    dist = newDist;
                    targetFoodIdx = i;
                }
            }
        }

        sensor.AddObservation(pos);
        sensor.AddObservation(gameboard.foodPos[targetFoodIdx] - pos);
        sensor.AddObservation(dirVec);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        ChangeDirection(actionBuffers.DiscreteActions[0]);
        Move();     
    }

    private void ChangeDirection(int action)
    {
        switch (action)
        {
            case 0:
                // do nothing
                break;

            case 1: // turn left
                if (curDir == Direction.Up)
                {
                    nextDir = Direction.Left;
                    dirVec = new Vector2(-1, 0);
                }
                else if (curDir == Direction.Down)
                {
                    nextDir = Direction.Right;
                    dirVec = new Vector2(1, 0);
                }
                else if (curDir == Direction.Left)
                {
                    nextDir = Direction.Down;
                    dirVec = new Vector2(0, -1);
                }
                else if (curDir == Direction.Right)
                {
                    nextDir = Direction.Up;
                    dirVec = new Vector2(0, 1);
                }
                break;

            case 2: // turn right
                if (curDir == Direction.Up)
                {
                    nextDir = Direction.Right;
                    dirVec = new Vector2(1, 0);
                }
                else if (curDir == Direction.Down)
                {
                    nextDir = Direction.Left;
                    dirVec = new Vector2(-1, 0);
                }
                else if (curDir == Direction.Left)
                {
                    nextDir = Direction.Up;
                    dirVec = new Vector2(0, 1);
                }
                else if (curDir == Direction.Right)
                {
                    nextDir = Direction.Down;
                    dirVec = new Vector2(0, -1);
                }
                break;
        }
    }
    
    private void Move()
    {
        posList.Insert(0, pos);

        curDir = nextDir;
        pos += dirVec;

        if (isEasy)
        {
            AddReward(-0.05f);
        }

        bool eatFood = gameboard.TryEatFood(pos);
        if (eatFood)
        {
            AddReward(isEasy ? 1.0f : 0.5f);
            
            length++;
            targetFoodIdx = -1;
            opponentSnake.targetFoodIdx = -1;

            // grow the body by 1 unit
            GameObject bodyObj = Instantiate(bodyPrefab);
            bodyObj.transform.SetParent(transform);
            bodyList.Insert(0, bodyObj.transform);

            gameboard.SpawnFood(); // spawn new apple
        }

        if (posList.Count >= length)
        {
            posList.RemoveAt(posList.Count - 1);
        }

        if (gameboard.IsOutsideBoard(pos) || IsHitSelfBody())
        {
            SetReward(isEasy ? -0.2f : -1.0f);
            gameboard.reset = true;
        }
        else if (IsHitOpponentBody())
        {
            SetReward(isEasy ? -0.2f : -1.0f);
            gameboard.reset = true;
        }
        else if (IsHitHead())
        {
            SetReward(isEasy ? -0.2f : -1.0f);
            opponentSnake.SetReward(isEasy ? -0.2f : -1.0f);
            gameboard.reset = true;
        }

        // move body
        for (int i = 0; i < posList.Count; i++)
        {
            Vector3 tmp = new Vector3(posList[i].x, posList[i].y, 0);
            bodyList[i].transform.position = tmp + gameboard.posOffset;
        }

        transform.GetChild(0).eulerAngles = new Vector3(0, 0, Angle(dirVec));
        transform.GetChild(0).position = new Vector3(pos.x, pos.y, 0) + gameboard.posOffset;        

        if(!isEasy && !eatFood && !gameboard.reset)
        {
            if (targetFoodIdx != -1)
            {
                float dist = Vector2.Distance(pos, gameboard.foodPos[targetFoodIdx]);
                SetReward((30f - dist) / 30 * 0.05f); // set reward based on distance to the nearest apple
            }
        }
        else if (gameboard.reset)
        {
            opponentSnake.EndEpisode();
            EndEpisode();
        }
    }

    private float Angle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90.0f;
    }

    public Vector2 GetPos()
    {
        return pos;
    }

    public List<Vector2> GetAllPos()
    {
        List<Vector2> allPosList = new List<Vector2>() { pos };
        allPosList.AddRange(posList);
        return allPosList;
    }

    public bool IsHitHead()
    {
        return pos == opponentSnake.GetPos();
    }

    public bool IsHitSelfBody()
    {
        foreach (Vector2 bodyPos in posList)
        {
            if (pos == bodyPos)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsHitOpponentBody()
    {
        foreach (Vector2 bodyPos in opponentSnake.posList)
        {
            if (pos == bodyPos)
            {
                return true;
            }
        }
        return false;
    }

    public void Reset()
    {
        int childNum = transform.childCount;
        for (int i = 0; i < childNum - 2; i++)
        {
            Destroy(transform.GetChild(childNum - i - 1).gameObject);
        }

        length = 2;
        curDir = initDir;
        nextDir = initDir;
        dirVec = initDir == Direction.Up ? new Vector2(0, 1) : new Vector2(0, -1);
        targetFoodIdx = -1;
        timer = 0.0f;

        pos = initPos;
        transform.GetChild(0).eulerAngles = initRot;

        transform.GetChild(0).position = new Vector3(pos.x, pos.y, 0) + gameboard.transform.position;
        transform.GetChild(1).position = new Vector3(pos.x, pos.y - dirVec.y, 0) + gameboard.transform.position;
        
        posList = new List<Vector2>();
        posList.Add(initDir == Direction.Up ? new Vector2(10, 1) : new Vector2(10, 19));
        bodyList = new List<Transform>();
        bodyList.Add(transform.GetChild(1));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 2;
        }
        else
        {
            discreteActionsOut[0] = 0;
        }
    }
}
