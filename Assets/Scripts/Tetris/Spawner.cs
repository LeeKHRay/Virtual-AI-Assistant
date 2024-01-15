using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] tetrominoes;
    public GameObject garbage;

    private Transform spawnLoc;
    private Vector3 garbagePos;

    private bool spawnIShape = false;

    void Start()
    {
        spawnLoc = transform.parent.Find("Blocks");
        garbagePos = transform.parent.position;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Keypad0)) // cheat
        {
            //spawnIShape = true;
        }
    }

    public void Spawn()
    {
        int idx = Random.Range(0, tetrominoes.Length); // Randomly choose a shape

        // Spawn tetromino
        Vector3 pos = transform.position;
        if (spawnIShape)
        {
            Instantiate(tetrominoes[0], pos, Quaternion.identity, spawnLoc);
            spawnIShape = false;
        }
        else
        {
            Instantiate(tetrominoes[idx], pos, Quaternion.identity, spawnLoc);
        }
    }

    public void SpawnHeldTetromino(Transform heldTetromino, SpriteRenderer[] heldTetrominoSr, Dictionary<char, Vector3[]> blocksPos, Color[] colors)
    {
        int idx = Random.Range(0, tetrominoes.Length); // Randomly choose a shape
        char shape = tetrominoes[idx].name[0];
        Color color = colors[idx];

        heldTetromino.name = shape + "";
        for (int i = 0; i < 4; i++)
        {
            heldTetromino.GetChild(i).localPosition = blocksPos[shape][i];
            heldTetrominoSr[i].color = color;
        }
        heldTetromino.GetChild(4).localPosition = blocksPos[shape][4];
    }

    public GameObject SpawnGarbage(int garbagePosOffset)
    {
        int holePos = Random.Range(0, 10); // Randomly choose the poistion of the hole

        Vector3 pos = garbagePos + new Vector3(0, garbagePosOffset, 0);
        GameObject garbageGameObj = Instantiate(garbage, pos, Quaternion.identity, spawnLoc);

        if (holePos != 0)
        {
            garbageGameObj.transform.GetChild(holePos - 1).position -= new Vector3(holePos, 0, 0);
        }
        return garbageGameObj;
    }
}
