using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;

public class TetrominoAgent : Agent
{
    private Transform pivot = null;
    private TrainingTetrisGameboard gameboard;

    private bool stopped = false;
    private float moveTimeInterval = 0.5f;
    private float timer = 0.0f;
    private float fallDownTimer = 0.0f;
    private Vector2 curPos;
    private Vector2 dir;
    private int shape;
    private Vector2 bestPos;
    private Vector2 bestDir;
    private int bestShape;
    private float prevXDist = float.NegativeInfinity;

    void Awake()
    {
        gameboard = transform.parent.parent.GetComponent<TrainingTetrisGameboard>();
    }

    void Update()
    {
        if (!gameboard.reset)
        {
            if (transform.localPosition.y <= 18)
            {
                timer += Time.deltaTime;
                if (timer >= 0.1f)
                {
                    RequestDecision(); // take observations and perform actions
                    timer -= 0.1f;
                }
            }

            fallDownTimer += Time.deltaTime;
            if (fallDownTimer >= moveTimeInterval)
            {
                fallDownTimer -= moveTimeInterval;
                FallDown();
            }
        }
    }

    public override void Initialize()
    {
        pivot = transform.Find("Pivot");
        dir = new Vector2(0, 1);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!stopped)
        {
            (bestPos, bestDir, bestShape) = gameboard.BestMove(transform, dir, gameboard.heldTetromino); // predict
        }
        curPos = gameboard.RoundVec2(transform.position - gameboard.posOffset);
        shape = TrainingTetrisGameboard.shapeToIdx[gameObject.name[0]];
        prevXDist = Mathf.Abs(curPos.x - bestPos.x);
		
        sensor.AddObservation(curPos);
        sensor.AddObservation(dir);
        sensor.AddObservation(bestPos - curPos);
        sensor.AddObservation(bestDir);
        sensor.AddObservation(shape);
        sensor.AddObservation(bestShape);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if(!stopped && !gameboard.reset)
        {
            int action1 = actionBuffers.DiscreteActions[0];
            int action2 = actionBuffers.DiscreteActions[1];
            int action3 = actionBuffers.DiscreteActions[2];
            int action4 = actionBuffers.DiscreteActions[3];

            if (action1 == 1) // swap
            {
                gameboard.SwapTetromino(transform);

                if (!IsValidGridPos())
                {
                    gameboard.SwapTetromino(transform);
                }
                else
                {
                    dir = gameboard.RoundVec2(Quaternion.Euler(0, 0, transform.eulerAngles.z) * new Vector2(0, 1));
                    shape = TrainingTetrisGameboard.shapeToIdx[gameObject.name[0]];
                }
            }

            if (action2 == 1) // rotate
            {
                transform.RotateAround(pivot.position, Vector3.forward, -90);

                if (!IsValidGridPos())
                {
                    transform.RotateAround(pivot.position, Vector3.forward, 90);
                }
                else
                {
                    dir = gameboard.RoundVec2(Quaternion.Euler(0, 0, -90) * dir);
                }
            }

            if (action3 == 1) // move to left
            {
                transform.position += new Vector3(-1, 0, 0);

                if (!IsValidGridPos())
                {
                    transform.position += new Vector3(1, 0, 0);
                }
            }
            else if (action3 == 2) // move to right
            {
                transform.position += new Vector3(1, 0, 0);

                if (!IsValidGridPos())
                {
                    transform.position += new Vector3(-1, 0, 0);
                }
            }

            curPos = gameboard.RoundVec2(transform.position - gameboard.posOffset);
            float xDist = Mathf.Abs(curPos.x - bestPos.x);
            if (action4 == 1) // drop quickly
            {
                if ((int)curPos.x != (int)bestPos.x || !dir.Equals(bestDir) || shape != bestShape)
                {
                    AddReward(-1.0f);
                }

                moveTimeInterval = 0.05f;
                fallDownTimer %= 0.05f;
            }
            else // drop at normal speed
            {
                if ((int)curPos.x == (int)bestPos.x && dir.Equals(bestDir) && shape == bestShape)
                {
                   AddReward(-1.0f);
                }
                moveTimeInterval = 0.5f;
            }

            if (shape == bestShape)
            {
                if (xDist < prevXDist || (int)curPos.x == (int)bestPos.x)
                {
                    prevXDist = xDist;
                    AddReward(0.5f);
                }
                else
                {                    
					SetReward(-1.0f);
                }

                if (dir.Equals(bestDir))
                {
                    AddReward(0.5f);
                }
                else
                {                    
					SetReward(-1.0f);
                }
            }
            else
            {
                SetReward(-1.0f);
            }
        }
    }

    private void FallDown()
    {
        transform.position += new Vector3(0, -1, 0);

        if (!IsValidGridPos())
        {
            transform.position += new Vector3(0, 1, 0);

            UpdateGrid(); 
            
            gameboard.RemoveFullRows();

            stopped = true;

            if (IsLose())
            {
                gameboard.reset = true;

                Debug.Log("GAME OVER");
            }
            else
            {
                gameboard.spawner.Spawn(); // spawn a new tetromino
            }
            EndEpisode();
            enabled = false;
        }
    }

    private bool IsValidGridPos()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.tag.Equals("Block"))
            {
                Vector2 pos = gameboard.RoundVec2(child.position - gameboard.posOffset);

                // check if block is inside gameboard
                if (!gameboard.IsInsideBoard(pos))
                {
                    return false;
                }

                // check if it is overlapping with other blocks
                if (gameboard.grid[(int)pos.x, (int)pos.y] != null &&
                    gameboard.grid[(int)pos.x, (int)pos.y].parent != transform)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsLose()
    {
        for (int i = 0; i < 10; i++)
        {
            if (gameboard.heights[i] >= 21) // check if the height of one of the columns exceeds 20 
            {
                return true;
            }
        }
        return false;
    }      

    private void UpdateGrid()
    {
        // add new blocks to grid
        foreach (Transform child in transform)
        {
            if (child.gameObject.tag.Equals("Block"))
            {
                Vector2 pos = gameboard.RoundVec2(child.position - gameboard.posOffset);
                gameboard.grid[(int)pos.x, (int)pos.y] = child;
            }
        }
        gameboard.UpdateHeights();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[0] = 1;
        }

        discreteActionsOut[1] = 0;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[1] = 1;
        }

        discreteActionsOut[2] = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[2] = 1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[2] = 2;
        }

        discreteActionsOut[3] = 0;
        if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[3] = 1;
        }
    }

    public void PrintGrid()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 22; j++)
            {
                if (gameboard.grid[i, j] != null)
                    Debug.Log("Grid: " +　i + ", " + j);
            }
        }
    }
}
