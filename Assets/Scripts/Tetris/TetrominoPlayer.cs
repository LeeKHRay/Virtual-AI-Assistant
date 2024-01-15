using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;

public class TetrominoPlayer : Agent
{
    private Transform pivot = null;
    private TetrisGameboard gameboard;

    private float moveTimeInterval = 0.5f;
    private float timer = 0.0f;
    private float fallDownTimer = 0.0f;
    private int[] actions;

    void Awake()
    {
        gameboard = transform.parent.parent.GetComponent<TetrisGameboard>();
    }

    void Update()
    {
        if (TetrisSceneManager.state == TetrisSceneManager.State.Normal)
        {
            if (Input.GetKeyDown(KeyCode.Space)) // swap
            {
                actions[0] = 1;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) // rotate
            {
                actions[1] = 1;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) // move to left
            {
                actions[2] = 1;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow)) // move to right
            {
                actions[2] = 2;
            }
            if (Input.GetKey(KeyCode.DownArrow)) // drop quickly
            {
                actions[3] = 1;
            }

            timer += Time.deltaTime;
            if (timer >= 0.1f)
            {
                RequestDecision();
                timer -= 0.1f;
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
        actions = new int[4] { 0, 0, 0, 0 };
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int action1 = actionBuffers.DiscreteActions[0];
        int action2 = actionBuffers.DiscreteActions[1];
        int action3 = actionBuffers.DiscreteActions[2];
        int action4 = actionBuffers.DiscreteActions[3];

        if (action1 == 1)
        {
            gameboard.SwapTetromino(transform);

            if (!IsValidGridPos())
            {
                gameboard.SwapTetromino(transform);
            }
            else
            {
                gameboard.SoundEffect("swap");
            }
        }

        if (action2 == 1)
        {
            transform.RotateAround(pivot.position, Vector3.forward, -90);

            if (!IsValidGridPos())
            {
                transform.RotateAround(pivot.position, Vector3.forward, 90);
            }
            else
            {
                gameboard.SoundEffect("rotate");
            }
        }

        if (action3 == 1)
        {
            transform.position += new Vector3(-1, 0, 0);

            if (!IsValidGridPos())
            {
                transform.position += new Vector3(1, 0, 0);
            }
        }
        else if (action3 == 2)
        {
            transform.position += new Vector3(1, 0, 0);

            if (!IsValidGridPos())
            {
                transform.position += new Vector3(-1, 0, 0);
            }
        }

        if (action4 == 1)
        {
            moveTimeInterval = 0.05f;
            fallDownTimer %= 0.05f;
        }
        else
        {
            moveTimeInterval = 0.5f;
        }
    }

    private void FallDown()
    {
        transform.position += new Vector3(0, -1, 0);

        if (!IsValidGridPos())
        {
            transform.position += new Vector3(0, 1, 0);
            
            UpdateGrid();

            int lines = gameboard.RemoveFullRows();
            if (lines > 0)
            {
                if (lines == 4)
                {
                    gameboard.SoundEffect("clearLine2");
                }
                else
                {
                    gameboard.SoundEffect("clearLine1");
                }
            }
            else
            {
                gameboard.SoundEffect("drop");
            }

            if (IsLose())
            {
                gameboard.lose = true;
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
            if (gameboard.heights[i] >= 21)
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

    // changing Behavior Type to "Heuristic Only" in Behavior Parameters in inspector
    // can use keyboard to control the agent
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = actions[0];
        actions[0] = 0;

        discreteActionsOut[1] = actions[1];
        actions[1] = 0;

        discreteActionsOut[2] = actions[2];
        actions[2] = 0;

        discreteActionsOut[3] = actions[3];
        actions[3] = 0;
    }
}
