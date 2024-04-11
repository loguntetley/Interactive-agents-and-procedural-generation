using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NUnit.Framework.Internal;
using static UnityEditor.PlayerSettings;
using Unity.VisualScripting;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject islandTile, OceanTile;
    [SerializeField] private int width;
    [SerializeField] private int height;
    private int[,] map, checkingMap;
    public GameObject[,] generatedMap;
    public int regionCount = 0;
    public List<List<Pair>> regions;

    [Range(0, 100)] [SerializeField] public int randomFillPercent;

    [SerializeField] private string seed;
    [SerializeField] private bool useRandomSeed;

    public struct Pair
    {
        public int x, z;

        public Pair(int x, int z) 
        {
            this.x = x;
            this.z = z;
        }
    }

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
        checkingMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < boarderMap.GetLength(0); x++)
        {
            for (int y = 0; y < boarderMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                    boarderMap[x, y] = map[x - borderSize, y - borderSize];
                else
                    boarderMap[x, y] = 1;
                checkingMap[x, y] = 0;
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
                    generatedMap[x, z] = Instantiate(islandTile, pos, Quaternion.identity);
                else
                    generatedMap[x, z] = Instantiate(OceanTile, pos, Quaternion.identity);

                generatedMap[x, z].transform.SetParent(this.gameObject.transform);
            }
        }

        for (int x = 0; x < boarderMap.GetLength(0); x++)
        {
            for (int z = 0; z < boarderMap.GetLength(1); z++)
            {
                if (boarderMap[x, z] == 0 && generatedMap[x, z].GetComponent<Tile>().region == 0 && checkingMap[x, z] == 0)
                {
                    regionCount++;
                    SetRegions(regionCount, boarderMap, x, z);                 
                }  
            }
        }

        regions = new List<List<Pair>>();
        SaveRegions();
    }

    private void SaveRegions()
    {
        for (int i = 1; i < regionCount + 1; i++)
        {
            List<Pair> region = new List<Pair>();
            for (int x = 0; x < generatedMap.GetLength(0); x++)
            {
                for (int z = 0; z < generatedMap.GetLength(1); z++)
                {
                    if (generatedMap[x, z].GetComponent<Tile>().region == i)
                    {
                        region.Add(new Pair(x, z));
                    }
                }
            }
            regions.Add(region);
        }
    }

    private void SetRegions(int regionCount, int[,] boarderMap, int positionCheckingX, int positionCheckingZ)
    {
        if (MapInCheckInBounds(positionCheckingX, positionCheckingZ))
        {
            generatedMap[positionCheckingX, positionCheckingZ].GetComponent<Tile>().region = regionCount;
            checkingMap[positionCheckingX, positionCheckingZ] = 1;
        }
        if (MapInCheckInBounds(positionCheckingX + 1, positionCheckingZ))
        {
            if (generatedMap[positionCheckingX + 1, positionCheckingZ].tag == "Island" && generatedMap[positionCheckingX + 1, positionCheckingZ].GetComponent<Tile>().region == 0 && checkingMap[positionCheckingX + 1, positionCheckingZ] == 0)
            {
                generatedMap[positionCheckingX + 1, positionCheckingZ].GetComponent<Tile>().region = regionCount;
                checkingMap[positionCheckingX + 1, positionCheckingZ] = 1;
                SetRegions(regionCount, boarderMap, positionCheckingX + 1, positionCheckingZ);
            }
        }
        if (MapInCheckInBounds(positionCheckingX - 1, positionCheckingZ))
        {
            if (generatedMap[positionCheckingX - 1, positionCheckingZ].tag == "Island" && generatedMap[positionCheckingX - 1, positionCheckingZ].GetComponent<Tile>().region == 0 && checkingMap[positionCheckingX - 1, positionCheckingZ] == 0)
            {
                generatedMap[positionCheckingX - 1, positionCheckingZ].GetComponent<Tile>().region = regionCount;
                checkingMap[positionCheckingX - 1, positionCheckingZ] = 1;
                SetRegions(regionCount, boarderMap, positionCheckingX - 1, positionCheckingZ);
            }
        }
        if (MapInCheckInBounds(positionCheckingX, positionCheckingZ + 1))
        {
            if (generatedMap[positionCheckingX, positionCheckingZ + 1].tag == "Island" && generatedMap[positionCheckingX, positionCheckingZ + 1].GetComponent<Tile>().region == 0 && checkingMap[positionCheckingX, positionCheckingZ + 1] == 0)
            {
                generatedMap[positionCheckingX, positionCheckingZ + 1].GetComponent<Tile>().region = regionCount;
                checkingMap[positionCheckingX, positionCheckingZ + 1] = 1;
                SetRegions(regionCount, boarderMap, positionCheckingX, positionCheckingZ + 1);
            }
        }
        if (MapInCheckInBounds(positionCheckingX, positionCheckingZ - 1))
        {
            if (generatedMap[positionCheckingX, positionCheckingZ - 1].tag == "Island" && generatedMap[positionCheckingX, positionCheckingZ - 1].GetComponent<Tile>().region == 0 && checkingMap[positionCheckingX, positionCheckingZ - 1] == 0)
            {
                generatedMap[positionCheckingX, positionCheckingZ - 1].GetComponent<Tile>().region = regionCount;
                checkingMap[positionCheckingX, positionCheckingZ - 1] = 1;
                SetRegions(regionCount, boarderMap, positionCheckingX, positionCheckingZ - 1);
            }
        }
        if (MapInCheckInBounds(positionCheckingX + 1, positionCheckingZ + 1))
        {
            if (generatedMap[positionCheckingX + 1, positionCheckingZ + 1].tag == "Island" && generatedMap[positionCheckingX + 1, positionCheckingZ + 1].GetComponent<Tile>().region == 0 && checkingMap[positionCheckingX + 1, positionCheckingZ + 1] == 0)
            {
                generatedMap[positionCheckingX + 1, positionCheckingZ + 1].GetComponent<Tile>().region = regionCount;
                checkingMap[positionCheckingX + 1, positionCheckingZ + 1] = 1;
                SetRegions(regionCount, boarderMap, positionCheckingX + 1, positionCheckingZ + 1);
            }
        }
        if (MapInCheckInBounds(positionCheckingX - 1, positionCheckingZ - 1))
        {
            if (generatedMap[positionCheckingX - 1, positionCheckingZ - 1].tag == "Island" && generatedMap[positionCheckingX - 1, positionCheckingZ - 1].GetComponent<Tile>().region == 0 && checkingMap[positionCheckingX - 1, positionCheckingZ - 1] == 0)
            {
                generatedMap[positionCheckingX - 1, positionCheckingZ - 1].GetComponent<Tile>().region = regionCount;
                checkingMap[positionCheckingX - 1, positionCheckingZ - 1] = 1;
                SetRegions(regionCount, boarderMap, positionCheckingX - 1, positionCheckingZ - 1);
            }
        }
        if (MapInCheckInBounds(positionCheckingX + 1, positionCheckingZ - 1))
        {
            if (generatedMap[positionCheckingX + 1, positionCheckingZ - 1].tag == "Island" && generatedMap[positionCheckingX + 1, positionCheckingZ - 1].GetComponent<Tile>().region == 0 && checkingMap[positionCheckingX + 1, positionCheckingZ - 1] == 0)
            {
                generatedMap[positionCheckingX + 1, positionCheckingZ - 1].GetComponent<Tile>().region = regionCount;
                checkingMap[positionCheckingX + 1, positionCheckingZ - 1] = 1;
                SetRegions(regionCount, boarderMap, positionCheckingX + 1, positionCheckingZ - 1);
            }
        }
        if (MapInCheckInBounds(positionCheckingX - 1, positionCheckingZ + 1))
        {
            if (generatedMap[positionCheckingX - 1, positionCheckingZ + 1].tag == "Island" && generatedMap[positionCheckingX - 1, positionCheckingZ + 1].GetComponent<Tile>().region == 0 && checkingMap[positionCheckingX - 1, positionCheckingZ + 1] == 0)
            {
                generatedMap[positionCheckingX - 1, positionCheckingZ + 1].GetComponent<Tile>().region = regionCount;
                checkingMap[positionCheckingX - 1, positionCheckingZ + 1] = 1;
                SetRegions(regionCount, boarderMap, positionCheckingX - 1, positionCheckingZ + 1);
            }
        }
    }

    public bool MapInCheckInBounds(int x, int z)
    {
        return (x <= generatedMap.GetLength(0) && x >= 0 && z <= generatedMap.GetLength(1) && z >= 0);    
    }

    private void RandomFillMap()
    {
        if (useRandomSeed)
            seed = System.DateTime.Now.Ticks.ToString();

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
