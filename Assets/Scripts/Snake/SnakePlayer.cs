using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;
using System;

public class SnakePlayer : Agent
{
    public enum Direction
    {
        Up, Down, Left, Right
    }

    public GameObject bodyPrefab;
    public SnakeOpponent opponentSnake;
    public Vector2 initPos;
    public Vector3 initRot;
    public Direction initDir;

    private Direction curDir;
    private Direction nextDir;
    private Vector2 pos;
    private Vector2 dirVec;
    public float moveTimeInterval = 0.4f;
    private float timer = 0.0f;
    private SnakeGameboard gameboard;
    public int length = 2;
    public List<Vector2> posList;
    private List<Transform> bodyList;
    public bool lose = false;

    void Awake()
    {
        gameboard = transform.parent.GetComponent<SnakeGameboard>();
    }

    void Update()
    {
        if (gameboard.state == SnakeGameboard.State.Normal)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && curDir != Direction.Down)
            {
                nextDir = Direction.Up;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && curDir != Direction.Up)
            {
                nextDir = Direction.Down;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && curDir != Direction.Right)
            {
                nextDir = Direction.Left;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && curDir != Direction.Left)
            {
                nextDir = Direction.Right;
            }

            timer += Time.deltaTime;
            if (timer >= moveTimeInterval)
            {
                timer -= moveTimeInterval;
                RequestDecision();
            }
        }
    }

    public override void Initialize()
    {
        Reset();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        ChangeDirection(actionBuffers.DiscreteActions[0]);
        Move();     
    }

    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        if (nextDir == Direction.Up || nextDir == Direction.Down)
        {
            actionMasker.SetMask(0, new int[2] { 1, 2 });
        }
        else if (nextDir == Direction.Left || nextDir == Direction.Right)
        {
            actionMasker.SetMask(0, new int[2] { 3, 4 });
        }
    }

    private void ChangeDirection(int action)
    {
        switch (action)
        {
            case 0:
                // do nothing
                break;
            case 1:
                if (curDir != Direction.Down)
                {
                    nextDir = Direction.Up;
                    dirVec = new Vector2(0, 1);
                }
                break;
            case 2:
                if (curDir != Direction.Up)
                {
                    nextDir = Direction.Down;
                    dirVec = new Vector2(0, -1);
                }
                break;
            case 3:
                if (curDir != Direction.Right)
                {
                    nextDir = Direction.Left;
                    dirVec = new Vector2(-1, 0);
                }
                break;
            case 4:
                if (curDir != Direction.Left)
                {
                    nextDir = Direction.Right;
                    dirVec = new Vector2(1, 0);
                }
                break;
        }
    }

    private void Move()
    {
        
        posList.Insert(0, pos);

        curDir = nextDir;
        pos += dirVec;

        bool eatFood = gameboard.TryEatFood(pos);
        if (eatFood)
        {
            gameboard.SoundEffect("eat");

            length++;

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

        if (gameboard.IsOutsideBoard(pos) || IsHitSelfBody() || IsHitOpponentBody())
        {
            lose = true;
            gameboard.state = SnakeGameboard.State.Lose;
        }
        else if (IsHitHead())
        {
            gameboard.state = SnakeGameboard.State.Draw;
        }

        // move body
        for (int i = 0; i < posList.Count; i++)
        {
            Vector3 tmp = new Vector3(posList[i].x, posList[i].y, 0);
            bodyList[i].transform.position = tmp + gameboard.posOffset;
        }

        transform.GetChild(0).eulerAngles = new Vector3(0, 0, Angle(dirVec));
        transform.GetChild(0).position = new Vector3(pos.x, pos.y, 0) + gameboard.posOffset;

        if (gameboard.state != SnakeGameboard.State.Normal)
        {
            EndEpisode();
        }

    }

    private float Angle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90.0f;
    }

    public bool IsHitHead()
    {
        return pos == opponentSnake.GetPos();
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
        List<Vector2> bodyPos = opponentSnake.posList;
        for (int i = 0; i < opponentSnake.posList.Count - 1; i++)
        {
            if (pos == bodyPos[i])
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
            Destroy(transform.GetChild(childNum - i - 1).gameObject); // remove grown body
        }

        length = 2;
        curDir = initDir;
        nextDir = initDir;
        dirVec = initDir == Direction.Up ? new Vector2(0, 1) : new Vector2(0, -1);
        timer = 0.0f;
        lose = false;

        pos = initPos;
        transform.GetChild(0).eulerAngles = initRot;
        transform.GetChild(0).position = new Vector3(pos.x, pos.y, 0) + gameboard.transform.position;
        transform.GetChild(1).position = new Vector3(pos.x, pos.y - dirVec.y, 0) + gameboard.transform.position;
        
        posList = new List<Vector2>();
        posList.Add(new Vector2(10, 1));
        bodyList = new List<Transform>();
        bodyList.Add(transform.GetChild(1));
    }

    // changing Behavior Type to "Heuristic Only" in Behavior Parameters in inspector
    // can use keyboard to control the agent
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (nextDir == Direction.Up)
        {
            discreteActionsOut[0] = 1;
        }
        else if (nextDir == Direction.Down)
        {
            discreteActionsOut[0] = 2;
        }
        else if (nextDir == Direction.Left)
        {
            discreteActionsOut[0] = 3;
        }
        else if (nextDir == Direction.Right)
        {
            discreteActionsOut[0] = 4;
        }
    }
}
