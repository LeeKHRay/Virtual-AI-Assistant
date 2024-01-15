using System.Collections.Generic;
using UnityEngine;

public class TrainingTetrisGameboard : MonoBehaviour
{
    public static Dictionary<char, int> shapeToIdx = new Dictionary<char, int>()
        { {'I', 0}, {'J', 1}, {'L', 2}, {'O', 3}, {'S', 4}, {'T', 5}, {'Z', 6} };

    private static Dictionary<char, int> rotationNum = new Dictionary<char, int>()
        { {'I', 2}, {'J', 4}, {'L', 4}, {'O', 1}, {'S', 2}, {'T', 4}, {'Z', 2} };

    private static Dictionary<char, Vector3[]> blocksPivotPos = new Dictionary<char, Vector3[]>()
        { {'I', new Vector3[5]},
        {'J', new Vector3[5]},
        {'L', new Vector3[5]},
        {'O', new Vector3[5]},
        {'S', new Vector3[5]},
        {'T', new Vector3[5]},
        {'Z', new Vector3[5]} };

    public Color[] colors = new Color[7];

    public static int width = 10;
    public static int height = 20;
    public Transform[,] grid = new Transform[width, height + 2];
    public Transform[,] predictGrid = new Transform[width, height + 2];

    public Transform heldTetromino;
    private Vector3 holdPos;
    private SpriteRenderer[] heldTetrominoSr;
    public TrainingTetrisGameboard opponentGameboard;

    public Vector3 posOffset;
    public Spawner spawner;
    public Transform blocks;
    public bool reset = false;
    public bool isBattle = false;

    public int[] heights;
    private int aggregateHeight = 0;
    private int lines = 0;
    private int holes = 0;
    private int bumpiness = 0;
    private Vector4 constants = new Vector4(-0.51f, 0.76f, -0.36f, -0.18f);

    private bool spawn = true;

    void Start()
    {
        posOffset = transform.position;
        heldTetrominoSr = new SpriteRenderer[4];
        holdPos = heldTetromino.position;

        // initialize pivot position and block position relative to pivot
        blocksPivotPos['I'][0] = new Vector3(-1, 0, 0);
        blocksPivotPos['I'][1] = new Vector3(0, 0, 0);
        blocksPivotPos['I'][2] = new Vector3(1, 0, 0);
        blocksPivotPos['I'][3] = new Vector3(2, 0, 0);
        blocksPivotPos['I'][4] = new Vector3(0.5f, -0.5f, 0);

        blocksPivotPos['J'][0] = new Vector3(-1, 1, 0);
        blocksPivotPos['J'][1] = new Vector3(-1, 0, 0);
        blocksPivotPos['J'][2] = new Vector3(0, 0, 0);
        blocksPivotPos['J'][3] = new Vector3(1, 0, 0);
        blocksPivotPos['J'][4] = new Vector3(0, 0, 0);

        blocksPivotPos['L'][0] = new Vector3(1, 1, 0);
        blocksPivotPos['L'][1] = new Vector3(1, 0, 0);
        blocksPivotPos['L'][2] = new Vector3(0, 0, 0);
        blocksPivotPos['L'][3] = new Vector3(-1, 0, 0);
        blocksPivotPos['L'][4] = new Vector3(0, 0, 0);

        blocksPivotPos['O'][0] = new Vector3(0, 1, 0);
        blocksPivotPos['O'][1] = new Vector3(1, 1, 0);
        blocksPivotPos['O'][2] = new Vector3(0, 0, 0);
        blocksPivotPos['O'][3] = new Vector3(1, 0, 0);
        blocksPivotPos['O'][4] = new Vector3(0.5f, 0.5f, 0);

        blocksPivotPos['S'][0] = new Vector3(1, 1, 0);
        blocksPivotPos['S'][1] = new Vector3(0, 1, 0);
        blocksPivotPos['S'][2] = new Vector3(0, 0, 0);
        blocksPivotPos['S'][3] = new Vector3(-1, 0, 0);
        blocksPivotPos['S'][4] = new Vector3(0, 0, 0);

        blocksPivotPos['T'][0] = new Vector3(0, 1, 0);
        blocksPivotPos['T'][1] = new Vector3(-1, 0, 0);
        blocksPivotPos['T'][2] = new Vector3(0, 0, 0);
        blocksPivotPos['T'][3] = new Vector3(1, 0, 0);
        blocksPivotPos['T'][4] = new Vector3(0, 0, 0);

        blocksPivotPos['Z'][0] = new Vector3(-1, 1, 0);
        blocksPivotPos['Z'][1] = new Vector3(0, 1, 0);
        blocksPivotPos['Z'][2] = new Vector3(0, 0, 0);
        blocksPivotPos['Z'][3] = new Vector3(1, 0, 0);
        blocksPivotPos['Z'][4] = new Vector3(0, 0, 0);

        for (int i = 0; i < 4; i++)
        {
            heldTetrominoSr[i] = heldTetromino.GetChild(i).GetComponent<SpriteRenderer>();
        }

        spawner.SpawnHeldTetromino(heldTetromino, heldTetrominoSr, blocksPivotPos, colors);
    }

    void Update()
    {
        heldTetromino.position = holdPos;

        if (spawn)
        {
            spawner.Spawn();
            spawn = false;
        }
        if (reset)
        {
            Reset();
        }
    }

    public void Reset()
    {
        // remove blocks in gameboard
        for (int y = height + 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null) 
                {
                    Destroy(grid[x, y].gameObject);
                    grid[x, y] = null;
                }
            }
        }

        GameObject[] tmpBlocks = new GameObject[blocks.childCount];
        int i = 0;

        foreach (Transform block in blocks)
        {
            tmpBlocks[i] = block.gameObject;
            i++;
        }
        foreach (GameObject block in tmpBlocks)
        {
            Destroy(block);
        }

        heights = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        aggregateHeight = 0;
        lines = 0;
        holes = 0;
        bumpiness = 0;

        reset = false;
        spawn = true;

        spawner.SpawnHeldTetromino(heldTetromino, heldTetrominoSr, blocksPivotPos, colors);
    }

    public Vector2 RoundVec2(Vector2 v)
    {
        return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    }

    public bool IsInsideBoard(Vector2 pos)
    {
        return ((int)pos.x >= 0 && (int)pos.x < width && (int)pos.y >= 0 && (int)pos.y < height + 2);
    }

    public void DropRows(int yInit)
    {
        for (int y = yInit; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    // Move one block downward
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = null;

                    // Update Block position
                    grid[x, y - 1].position += new Vector3(0, -1, 0);
                }
            }
        }
    }

    public bool IsRowFull(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }

    public void DeleteRow(int y)
    {
        for (int x = 0; x < width; x++)
        {
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    public int RemoveFullRows()
    {
        int fullRowsNum = 0;
        for (int y = 0; y < height; y++)
        {
            if (IsRowFull(y))
            {
                DeleteRow(y);
                DropRows(y + 1);
                y--;
                fullRowsNum++;
            }
        }

        UpdateHeights();

        return fullRowsNum;
    }

    private bool IsRightMost(Transform tetromino, Vector2[] blockPos, int x)
    {
        foreach (Vector2 pos in blockPos)
        {
            if ((int)pos.x + x >= width || 
                (grid[(int)pos.x + x, (int)pos.y] != null &&
                grid[(int)pos.x + x, (int)pos.y].parent != tetromino))
            {
                return true;
            }
        }
        return false;
    }

    public (Vector2, Vector2, int) BestMove(Transform tetromino, Vector2 dir, Transform heldTetromino)
    {
        Vector2[] tetrominoesDir = new Vector2[2] { dir, Quaternion.Euler(0, 0, transform.eulerAngles.z) * new Vector2(0, 1) };
        float bestFitness = float.NegativeInfinity;
        Vector2 bestPos = new Vector2();
        Vector2 bestDir = new Vector2();
        int bestShape = 0;

        bool isSameShape = tetromino.name[0] == heldTetromino.name[0];
        for (int idx = 0; idx < (isSameShape ? 1 : 2); idx++)
        {
            char shape = tetromino.name[0];
            Vector2[] tetrominoPos = new Vector2[4];
            Vector2[][] blockPos = new Vector2[4][];

            Transform pivot = tetromino.GetChild(4);
            bool[] validPos = { true, true, true, true };

            // calculate blocks position after rotations
            for (int i = 0; i < rotationNum[shape]; i++)
            {
                blockPos[i] = new Vector2[4];
                tetrominoPos[i] = RoundVec2(Quaternion.Euler(0, 0, -90 * i) * (tetromino.position - pivot.position) + pivot.position - posOffset);

                for (int j = 0; j < 4; j++)
                {
                    Transform block = tetromino.GetChild(j);
                    blockPos[i][j] = RoundVec2(Quaternion.Euler(0, 0, -90 * i) * (block.position - pivot.position) + pivot.position - posOffset);
                    
                    if (!IsInsideBoard(blockPos[i][j]) ||
                    (grid[(int)blockPos[i][j].x, (int)blockPos[i][j].y] != null &&
                    grid[(int)blockPos[i][j].x, (int)blockPos[i][j].y].parent != tetromino))
                    {
                        validPos[i] = false;
                        break;
                    }
                }
            }

            // "move" blocks to the leftmost
            for (int i = 0; i < rotationNum[shape]; i++)
            {
                if (!validPos[i])
                {
                    continue;
                }

                bool atLeftmost = false;
                int x = 1;
                while (true)
                {
                    foreach (Vector2 pos in blockPos[i])
                    {
                        if ((int)pos.x - x < 0 ||
                            (grid[(int)pos.x - x, (int)pos.y] != null &&
                            grid[(int)pos.x - x, (int)pos.y].parent != tetromino))
                        {
                            atLeftmost = true;
                            break;
                        }
                    }
                    if (atLeftmost)
                    {
                        tetrominoPos[i] += new Vector2(-x + 1, 0);

                        for (int j = 0; j < 4; j++)
                        {
                            blockPos[i][j] += new Vector2(-x + 1, 0);
                        }
                        break;
                    }
                    x++;
                }
            }

            // find the best position and direction
            for (int i = 0; i < rotationNum[shape]; i++)
            {
                if (!validPos[i])
                {
                    continue;
                }
                int x = 0;
                while (!IsRightMost(tetromino, blockPos[i], x))
                {
                    bool atBottom = false;
                    int y = 1;
                    while (true)
                    {
                        foreach (Vector2 pos in blockPos[i])
                        {
                            if ((int)pos.y - y < 0 ||
                                (grid[(int)pos.x + x, (int)pos.y - y] != null &&
                                grid[(int)pos.x + x, (int)pos.y - y].parent != tetromino))
                            {
                                atBottom = true;
                                break;
                            }
                        }
                        if (atBottom)
                        {
                            y--;
                            break;
                        }
                        else
                        {
                            y++;
                        }
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        grid[(int)blockPos[i][j].x + x, (int)blockPos[i][j].y - y] = tetromino.GetChild(j);
                    }

                    // update parameters of fitness function
                    AggregateHeight();
                    LineNum();
                    Holes();
                    Bumpiness();

                    foreach (Vector2 pos in blockPos[i])
                    {
                        grid[(int)pos.x + x, (int)pos.y - y] = null;
                    }

                    float fitness = Fitness();
                    if (fitness > bestFitness)
                    {
                        bestFitness = fitness;
                        bestPos = tetrominoPos[i] + new Vector2(x, -y);
                        bestDir = RoundVec2(Quaternion.Euler(0, 0, -90 * i) * tetrominoesDir[idx]);
                        bestShape = shapeToIdx[shape];
                    }
                    x++;
                }
            }

            if (!isSameShape)
            {
                SwapTetromino(tetromino); // use another tetromino for prediction
            }
        }

        return (bestPos, bestDir, bestShape);
    }

    // fitness function
    public float Fitness()
    {
        Vector4 features = new Vector4(aggregateHeight, lines, holes, bumpiness);
        return Vector4.Dot(constants, features);
    }

    public void UpdateHeights()
    {
        heights = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        for (int x = 0; x < width; x++)
        {
            for (int y = height + 1; y >= 0; y--)
            {
                if (grid[x, y] != null)
                {
                    heights[x] = y + 1;
                    break;
                }
            }
        }
    }

    private void AggregateHeight()
    {
        aggregateHeight = 0;
        heights = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        for (int x = 0; x < width; x++)
        {
            for (int y = height + 1; y >= 0; y--)
            {
                if (grid[x, y] != null)
                {
                    aggregateHeight += y + 1;
                    heights[x] = y + 1;
                    break;
                }
            }
        }
    }

    public void LineNum()
    {
        lines = 0;
        for (int y = 0; y < height; y++)
        {
            if (IsRowFull(y))
            {
                lines++;
            }
        }
    }

    private void Holes()
    {
        holes = 0;
        for (int x = 0; x < width; x++)
        {
            if (heights[x] != 0)
            {
                for (int y = 0; y < height; y++)
                {
                    if (y + 1 >= heights[x])
                    {
                        break;
                    }
                    if (grid[x, y] == null)
                    {
                        holes++;
                    }
                }
            }
        }
    }

    private void Bumpiness()
    {
        bumpiness = 0;

        for (int i = 0; i < heights.Length - 1; i++)
        {
            bumpiness += Mathf.Abs(heights[i] - heights[i + 1]);
        }
    }

    public void SwapTetromino(Transform tetromino)
    {
        char heldTetrominoShape = heldTetromino.name[0];
        char shape = tetromino.name[0];

        heldTetromino.name = shape + "";
        tetromino.name = heldTetrominoShape + "";

        // swap color and positions of blocks
        for (int i = 0; i < 4; i++)
        {
            SpriteRenderer sr = tetromino.GetChild(i).GetComponent<SpriteRenderer>();
            heldTetrominoSr[i].color = colors[shapeToIdx[shape]];
            sr.color = colors[shapeToIdx[heldTetrominoShape]];

            heldTetromino.GetChild(i).localPosition = blocksPivotPos[shape][i];
            tetromino.GetChild(i).localPosition = blocksPivotPos[heldTetrominoShape][i];
        }

        // swap pivot position
        Transform heldTetrominoPivot = heldTetromino.GetChild(4);
        Transform pivot = tetromino.GetChild(4);
        heldTetrominoPivot.localPosition = blocksPivotPos[shape][4];
        pivot.localPosition = blocksPivotPos[heldTetrominoShape][4];

        // swap rotation
        Vector3 tetrominoPos = tetromino.position;
        float heldTetrominoRotAngle = heldTetromino.eulerAngles.z;
        float rotAngle = tetromino.eulerAngles.z;
        tetromino.RotateAround(pivot.position, Vector3.forward, -rotAngle);
        tetromino.RotateAround(pivot.position, Vector3.forward, heldTetrominoRotAngle);
        heldTetromino.RotateAround(heldTetrominoPivot.position, Vector3.forward, -heldTetrominoRotAngle);
        heldTetromino.RotateAround(heldTetrominoPivot.position, Vector3.forward, rotAngle);
        tetromino.position = tetrominoPos;
    }
}
