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

    ActionNode setTargetNode;
    ActionNode playerproximityCheckNode;

    Selector rootNode;
    Sequence playerNearbySequence;

    private void Start()
    {
        setTargetNode = new ActionNode(SetTarget);
        playerproximityCheckNode = new ActionNode(HostileTargetProximityCheck);
        playerNearbySequence = new Sequence(new List<Node> 
        {
            playerproximityCheckNode,
            setTargetNode,

        });

        rootNode = new Selector(new List<Node>
        {
            playerNearbySequence,
        });

        nav = GetComponent<NavMeshAgent>();
    }


    private void Update()
    {
        rootNode.Evaluate();

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

    }





}
