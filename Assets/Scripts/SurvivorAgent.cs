using NPBehave;
using System;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class SurvivorAgent : MonoBehaviour
{
    private NPBehave.Blackboard sharedBlackboard;
    private NPBehave.Blackboard ownBlackboard;
    private Root behaviorTree;
    private Vector3 explorePosition;
    private GameObject[,] map;
    private MapGenerator mapGenerator;
    private float speed = 8f;
    private float engagmentRange = 5f;
    public int region = 0;
    //[SerializeField] private Material testMaterial;

    private void Start()
    {
        explorePosition = new Vector3(9999f, 9999f, 9999f);
        sharedBlackboard = UnityContext.GetSharedBlackboard("survivor-ai");
        ownBlackboard = new NPBehave.Blackboard(sharedBlackboard, UnityContext.GetClock());
        behaviorTree = CreateBehaviourTree();
        behaviorTree.Start();
    }

    private Root CreateBehaviourTree()
    {
        return new Root(ownBlackboard,

            new Service(0.125f, UpdateBlackboards,

                new Selector(

                    new BlackboardCondition("flee", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,

                        new NPBehave.Sequence(

                            new NPBehave.Action(() => SetColor(Color.blue)),

                            new NPBehave.Action((bool _shouldCancel) =>
                            {
                                if (!_shouldCancel)
                                {
                                    FleeTarget(ownBlackboard.Get<Vector3>("zombiePosition"));
                                    return NPBehave.Action.Result.PROGRESS;
                                }
                                else
                                {
                                    return NPBehave.Action.Result.FAILED;
                                }
                            })
                        )
                    ),
                    new BlackboardCondition("exploring", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,

                        new NPBehave.Sequence(
                            new NPBehave.Action(() => SetColor(Color.green)),

                            new NPBehave.Action((bool _shouldCancel) =>
                            {
                                if (!_shouldCancel)
                                {
                                    Explore();
                                    //Debug.Log("Exploring");
                                    return NPBehave.Action.Result.PROGRESS;
                                }
                                else
                                {
                                    //Debug.Log("Stopped Exploring");
                                    return NPBehave.Action.Result.FAILED;
                                }
                            })
                            )
                        )
                    )
                )
            
        );
    }

    private void UpdateBlackboards()
    {
        GameObject[] zombiePositions = GameObject.FindGameObjectsWithTag("Zombie");

        float closestDistance = 9999f;
        int closestZombieIndex = 0;
        for (int i = 0; i < zombiePositions.Length; i++)
        {
            float distance = (this.transform.position - zombiePositions[i].transform.position).magnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestZombieIndex = i;
            }
        }

        ownBlackboard["zombiePosition"] = zombiePositions[closestZombieIndex].transform.position;
        ownBlackboard["zombieInRange"] = Vector3.Distance(zombiePositions[closestZombieIndex].transform.position, this.transform.position) < engagmentRange;

        if (ownBlackboard.Get<bool>("zombieInRange") && !ownBlackboard.Get<bool>("flee"))
        {
            ownBlackboard["flee"] = true;
            ownBlackboard["exploring"] = false;
        }

        if (!ownBlackboard.Get<bool>("zombieInRange") && ownBlackboard.Get<bool>("flee"))
        {
            ownBlackboard["flee"] = false;
            ownBlackboard["exploring"] = true;
        }

        if (!ownBlackboard.Get<bool>("zombieInRange") && !ownBlackboard.Get<bool>("flee"))
        {
            ownBlackboard["flee"] = false;
            ownBlackboard["exploring"] = true;
        }
    }

    private void SetColor(Color color)
    {
        GetComponent<MeshRenderer>().material.SetColor("_Color", color);
    }

    private void MoveTowardsTarget(Vector3 target)
    {
        float step = 5f * Time.deltaTime;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, new Vector3(target.x, target.y, target.z), step);
        transform.position = newPosition;
    }

    private void Explore()
    {
        if (explorePosition.y == 9999f)
        {
            mapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<MapGenerator>();
            map = mapGenerator.generatedMap;
            explorePosition = FindExplorablePosition();
        }

        transform.position = Vector3.MoveTowards(transform.position, explorePosition, speed * Time.deltaTime);
        //map[(int)explorePosition.x, (int)explorePosition.z].GetComponent<MeshRenderer>().material = testMaterial;

        if (transform.position.x == explorePosition.x && transform.position.z == explorePosition.z)
            explorePosition = FindExplorablePosition();    
    }

    private Vector3 FindExplorablePosition()
    {
        int randomTile = Random.Range(0, mapGenerator.regions[region].Count);
        return new Vector3(mapGenerator.regions[region][randomTile].x, 0.01f, mapGenerator.regions[region][randomTile].z);
    }

    private void FleeTarget(Vector3 objectPosition)
    {
        Vector3 directionFromAToB = (objectPosition - transform.position).normalized;
        Vector3 oppositeDirection = -directionFromAToB;
        Vector3 redirectedPosition = new Vector3(100, 0.0f, 100);//(transform.position.x * oppositeDirection.x, 0.01f, transform.position.z * oppositeDirection.z);
        Vector3 newPosition = Vector3.MoveTowards(transform.position, redirectedPosition, speed * Time.deltaTime);
        transform.position = newPosition;      
    }




}

