using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using BehaviorTrees;

public class Brian : NetworkBehaviour
{
    NavMeshAgent nav;
    public GameObject hostilityTarget;

    public SphereCollider hostilityArea;

    public GameObject wanderTarget;
    public float minDistanceToWanderTarget = 1;
    public float minChargeDistance = 10;

    public float walkSpeed = 3.5f;

    public float runSpeed = 5;

    ActionNode setTargetNode;


    Selector wanderSelector;

    Sequence wanderTargetSequence;
    Inverter wanderTargetNullCheckInverter;
    ActionNode setWanderTargetNode;

    Selector moveToWanderTargetSelector;

    Sequence atWanderTargetSequence;
    ActionNode atWanderTargetCheckInverter;
    ActionNode resetWanderTargetNode;

    ActionNode moveToTargetDestinationNode;

    ActionNode playerproximityCheckNode;

    Selector rootNode;
    Sequence playerNearbySequence;

    private void Start()
    {
        wanderTargetNullCheckInverter = new Inverter(new ActionNode(WanderTargetNullCheck));
        setWanderTargetNode = new ActionNode(SetNewWanderTarget);

        atWanderTargetCheckInverter = new ActionNode(AtWanderTargetCheck);
        resetWanderTargetNode = new ActionNode(ResetWanderTarget);

        moveToTargetDestinationNode = new ActionNode(MoveToTargetDestination);


        wanderTargetSequence = new Sequence(new List<Node>
        {
            wanderTargetNullCheckInverter,
            setWanderTargetNode,
        });

        atWanderTargetSequence = new Sequence(new List<Node>
        {
            atWanderTargetCheckInverter,
            resetWanderTargetNode,
        });

        moveToWanderTargetSelector = new Selector(new List<Node>
        {
            atWanderTargetSequence,
            moveToTargetDestinationNode,
        });

        wanderSelector = new Selector(new List<Node>
        {
            wanderTargetSequence,
            moveToWanderTargetSelector,
        });




        rootNode = new Selector(new List<Node>
        {
            wanderSelector
        });




        //setTargetNode = new ActionNode(SetTarget);
        //playerproximityCheckNode = new ActionNode(HostileTargetProximityCheck);
        //playerNearbySequence = new Sequence(new List<Node> 
        //{
        //    playerproximityCheckNode,
        //    setTargetNode,

        //});



        nav = GetComponent<NavMeshAgent>();
    }


    private void Update()
    {
        rootNode.Evaluate();

    }

    private NodeStates MoveToTargetDestination()
    {
        nav.enabled = true;
        nav.SetDestination(wanderTarget.transform.position);
        return NodeStates.SUCCESS;
    }

    private NodeStates WanderTargetNullCheck()
    {
        if (wanderTarget == null)
        {
            return NodeStates.FAILURE;
        }
        else
        {
            return NodeStates.SUCCESS;
        }
    }

    private NodeStates ResetWanderTarget()
    {
        Destroy(wanderTarget);
        nav.enabled = false;
        return NodeStates.SUCCESS;
    }

    private NodeStates SetNewWanderTarget()
    {
        wanderTarget = new GameObject("Wander Target");
        wanderTarget.transform.position = RandomNavSphere(transform.position, hostilityArea.radius, -1);
        return NodeStates.SUCCESS;
    }

    private NodeStates SetTarget()
    {
        if (nav.hasPath)
        {

            return NodeStates.SUCCESS;
        }
        else
        {
            nav.SetDestination(hostilityTarget.transform.position);
            return NodeStates.SUCCESS;
        }
    }

    private NodeStates HostileTargetProximityCheck()
    {
        if (hostilityTarget)
        {
            Debug.Log("Player is in range");
            return NodeStates.SUCCESS;
        }
        else
        {
            Debug.Log("Player is not in range");
            return NodeStates.FAILURE;
        }
    }

    private NodeStates AtWanderTargetCheck()
    {
        if (Vector3.Distance(transform.position, wanderTarget.transform.position) <= minDistanceToWanderTarget)
        {
            return NodeStates.SUCCESS;
        }

        return NodeStates.FAILURE;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;

        randomDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);

        return navHit.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called on Brian");
        if (hostilityTarget != null)
        {
            return;
        }
        if (other.GetComponent<PlayerCharacterController>())
        {
            hostilityTarget = other.gameObject;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == hostilityTarget)
        {
            hostilityTarget = null;
        }
    }



    private void OnDrawGizmos()
    {
        Color oldColor = Gizmos.color;
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, hostilityArea.radius);
        Gizmos.color = oldColor;



        if (wanderTarget != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, wanderTarget.transform.position);
            Gizmos.DrawWireSphere(wanderTarget.transform.position, 1);
            Gizmos.color = oldColor;
        }


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minChargeDistance);
        Gizmos.color = oldColor;

    }





}
