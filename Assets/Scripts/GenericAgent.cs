using NPBehave;
using System;
using System.Xml.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class GenericAgent : MonoBehaviour
{
    private NPBehave.Blackboard sharedBlackboard;
    private NPBehave.Blackboard ownBlackboard;
    private Root behaviorTree;
    private Vector3 explorePosition = new Vector3(999f, 999f, 999f);

    private void Start()
    {
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

                        new Sequence(

                            new NPBehave.Action(() => SetColor(Color.red)),

                            new NPBehave.Action((bool _shouldCancel) =>
                            {
                                if (!_shouldCancel)
                                {
                                    MoveTowardsTarget(ownBlackboard.Get<Vector3>("survivorPosition"));
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

                        new Sequence(
                            new NPBehave.Action(() => SetColor(Color.gray)),

                            new NPBehave.Action((bool _shouldCancel) =>
                            {
                                if (!_shouldCancel)
                                {
                                    Explore();
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

    private void UpdateBlackboards()
    {
        Vector3 survivorPosition = GameObject.FindGameObjectWithTag("Survivor").transform.position;

        Debug.Log(survivorPosition);
        ownBlackboard["survivorPosition"] = survivorPosition;
        ownBlackboard["survivorInRange"] = Vector3.Distance(survivorPosition, this.transform.position) < 7.5f;

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
        transform.position = newPosition;
    }

    private void Explore()
    {

        if (explorePosition.y != 0)
        {
            float x = Random.Range(-30, 30);
            float z = Random.Range(-30, 30);
            explorePosition = new Vector3(x, 0, z);
        }

        transform.position = Vector3.MoveTowards(transform.position, new Vector3(explorePosition.x, explorePosition.y, explorePosition.z), 5f * Time.deltaTime);

        if ((Vector3)transform.position == explorePosition)
        {
            float x = Random.Range(-30, 30);
            float z = Random.Range(-30, 30);
            explorePosition = new Vector3(x, 0, z);
        }
    }

    


}

