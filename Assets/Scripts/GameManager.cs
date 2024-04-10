using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject zombieAgent;
    [SerializeField] private GameObject survivorAgent;
    [SerializeField] private int zombieSpawnsPerRegion = 1;
    [SerializeField] private int survivorSpawnsPerRegion = 5;
    private MapGenerator mapGenerator;

    private void Start()
    {
        mapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<MapGenerator>();
        SpawnAgents();
    }

    private void SpawnAgents()
    {
        for (int i = 0; i < mapGenerator.regionCount; i++)
        {
           
            for (int j = 0; j < survivorSpawnsPerRegion; j++)
            {
                int randomTile = Random.Range(0, mapGenerator.regions[i].Count);
                Vector3 spawnPosition = new Vector3(mapGenerator.regions[i][randomTile].x, 0.01f, mapGenerator.regions[i][randomTile].z);
                GameObject survivor = Instantiate(survivorAgent, spawnPosition, Quaternion.identity);
                survivor.GetComponent<SurvivorAgent>().region = i;
            }

            for (int j = 0; j < zombieSpawnsPerRegion; j++)
            {
                int randomTile = Random.Range(0, mapGenerator.regions[i].Count);
                Vector3 spawnPosition = new Vector3(mapGenerator.regions[i][randomTile].x, 0.01f, mapGenerator.regions[i][randomTile].z);
                GameObject zombie = Instantiate(zombieAgent, spawnPosition, Quaternion.identity);
                zombie.GetComponent<ZombieAgent>().region = i;
            }
        } 
    }
}
