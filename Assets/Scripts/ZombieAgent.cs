using NPBehave;
using System;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class ZombieAgent : MonoBehaviour
{
    private NPBehave.Blackboard sharedBlackboard;
    private NPBehave.Blackboard ownBlackboard;
    private Root behaviorTree;
    private Vector3 explorePosition;
    private GameObject[,] map;
    private MapGenerator mapGenerator;
    private float speed = 5f;
    private float engagmentRange = 7.5f;
    private float biteRange = 3.5f;
    public int region = 0;
    [SerializeField] private Material testMaterial;
    [SerializeField] private GameObject testCube;

    private void Start()
    {
        explorePosition = new Vector3(9999f, 9999f, 9999f);
        sharedBlackboard = UnityContext.GetSharedBlackboard("zombie-ai");
        ownBlackboard = new NPBehave.Blackboard(sharedBlackboard, UnityContext.GetClock());
        behaviorTree = CreateBehaviourTree();
        behaviorTree.Start();
        
    }

    private Root CreateBehaviourTree()
    {
        return new Root(ownBlackboard,

            new Service(0.125f, UpdateBlackboards,

                new Selector(

                    new BlackboardCondition("engaged", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,

                        new NPBehave.Sequence(

                            new NPBehave.Action(() => SetColor(Color.red)),

                            new NPBehave.Action((bool _shouldCancel) =>
                            {
                                if (!_shouldCancel)
                                {
                                    MoveTowardsTarget(ownBlackboard.Get<Vector3>("survivorPosition"));
                                    AttemptBite(ownBlackboard.Get<GameObject>("survivor"));
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
                            new NPBehave.Action(() => SetColor(Color.magenta)),

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
                                    return NPBehave.Action.Result.FAILED;
                                }
                            })
                            )
                        )
                    )
                )
            
        );
    }

    private void AttemptBite(GameObject target)
    {
        if (Vector3.Distance(target.transform.position, this.transform.position) < biteRange)
        {
            //Debug.Log("Bite");
            target.GetComponent<SurvivorAgent>().bitten = true;
        }
    }

    private void UpdateBlackboards()
    {
        GameObject[] survivorPositions = GameObject.FindGameObjectsWithTag("Survivor");

        float closestDistance = 9999f;
        int closestSurvivorIndex = 0;
        for (int i = 0; i < survivorPositions.Length; i++)
        {
            float distance = (this.transform.position - survivorPositions[i].transform.position).magnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSurvivorIndex = i;
            }
        }

        ownBlackboard["survivor"] = survivorPositions[closestSurvivorIndex];
        ownBlackboard["survivorPosition"] = survivorPositions[closestSurvivorIndex].transform.position;
        ownBlackboard["survivorInRange"] = Vector3.Distance(survivorPositions[closestSurvivorIndex].transform.position, this.transform.position) < engagmentRange;

        if (ownBlackboard.Get<bool>("survivorInRange") && !ownBlackboard.Get<bool>("engaged"))
        {
            ownBlackboard["engaged"] = true;
            ownBlackboard["exploring"] = false;
        }

        if (!ownBlackboard.Get<bool>("survivorInRange") && ownBlackboard.Get<bool>("engaged"))
        {
            ownBlackboard["engaged"] = false;
            ownBlackboard["exploring"] = true;
        }

        if (!ownBlackboard.Get<bool>("survivorInRange") && !ownBlackboard.Get<bool>("engaged"))
        {
            ownBlackboard["engaged"] = false;
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
        if (map == null)
        {
            //Debug.Log("Map Set");
            mapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<MapGenerator>();
            map = mapGenerator.generatedMap;
        }
        if (map[(int)newPosition.x, (int)newPosition.z].tag == "Island")
        {
            transform.position = newPosition;
        }
        else
        {
            ownBlackboard["engaged"] = false;
            ownBlackboard["exploring"] = true;
        }
        
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
        map[(int)explorePosition.x, (int)explorePosition.z].GetComponent<MeshRenderer>().material = testMaterial;

        if (transform.position.x == explorePosition.x && transform.position.z == explorePosition.z)
            explorePosition = FindExplorablePosition();    
    }

    private Vector3 FindExplorablePosition()
    {
        int randomTile = Random.Range(0, mapGenerator.regions[region].Count);
        Vector3 newPosition = new Vector3(mapGenerator.regions[region][randomTile].x, 0.01f, mapGenerator.regions[region][randomTile].z);

        int i = 1;
        Vector3 offset = transform.position;
        while (PointNotReached(offset, newPosition))
        {
            i++;
            Vector3 direction = Vector3.Normalize(newPosition - transform.position);
            offset = transform.position + direction * 1f * i;       

            try
            {
                if (map[(int)offset.x, (int)offset.z].tag == "Ocean")
                {
                    return FindExplorablePosition();
                }
            }
            catch
            {
                return newPosition;
            }
        }

        return newPosition;

    }

    private bool PointNotReached(Vector3 currentPositionToSearch, Vector3 endPosition)
    {
        if (Vector3.Distance(currentPositionToSearch, endPosition) < 1.5f)
        {
            return false;
        }

        return true;
    }
}

