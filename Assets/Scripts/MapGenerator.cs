using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NUnit.Framework.Internal;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject islandTile, OceanTile;
    [SerializeField] private int width;
    [SerializeField] private int height;
    private int[,] map;
    public GameObject[,] generatedMap;

    [Range(0, 100)] [SerializeField] public int randomFillPercent;

    [SerializeField] private string seed;
    [SerializeField] private bool useRandomSeed;

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        int borderSize = 5;
        int[,] boarderMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < boarderMap.GetLength(0); x++)
        {
            for (int y = 0; y < boarderMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                    boarderMap[x, y] = map[x - borderSize, y - borderSize];
                else
                    boarderMap[x, y] = 1;

            }
        }

        //MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
        //meshGenerator.GenerateMesh(boarderMap, 1);
        generatedMap = new GameObject[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < boarderMap.GetLength(0); x++)
        {
            for (int z = 0; z < boarderMap.GetLength(1); z++)
            {
                Vector3 pos = new Vector3(x, 0, z);

                if (boarderMap[x, z] == 0)
                    generatedMap[x,z] = Instantiate(islandTile, pos, Quaternion.identity);
                else
                    generatedMap[x, z] =  Instantiate(OceanTile, pos, Quaternion.identity);

                generatedMap[x, z].transform.SetParent(this.gameObject.transform);
            }
        }


    }

    private void RandomFillMap()
    {
        if (useRandomSeed)
            seed = Time.time.ToString();

        System.Random randSeed = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++) 
        {
            for (int y = 0; y < height; y++) 
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    map[x, y] = 1;
                else
                    map[x, y] = (randSeed.Next(0, 100) < randomFillPercent) ? 1 : 0;
            }
        }
    }

    private void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;
            }
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;

        for (int neigbourX = gridX - 1; neigbourX <= gridX + 1; neigbourX++)
        {
            for (int neigbourY = gridY - 1; neigbourY <= gridY + 1; neigbourY++)
            {
                if (neigbourX >= 0 && neigbourX < width && neigbourY >= 0 && neigbourY < height)
                {
                    if (neigbourX != gridX || neigbourY != gridY)
                    {
                        wallCount += map[neigbourX, neigbourY];
                    }
                }
                else
                    wallCount++;

            }
        }

        return wallCount;
    }

    private void OnDrawGizmos()
    {
        /*if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x,y] == 1) ? Color.black: Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + 0.5f, 0, -height / 2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }*/
    }
}
