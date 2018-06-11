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

    public GameObject navAgentDestination;
    public float minDistanceToWanderTarget = 1;
    public float minChargeDistance = 10;

    public float walkSpeed = 3.5f;
    public float runSpeed = 5;

    public float timer = 0;
    public float chargeTime = 3;

    bool waiting = false;

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



    Sequence attackPlayerSequence;

    ActionNode hostileTargetNullCheckNode;


    Selector chargeSelector;
    Sequence chargeTargetDistanceCheckSequence;
    Inverter hostileTargetChargeDistanceCheckNode;
    ActionNode moveTowardsPlayerAction;
    

    Sequence chargeTargetCheckSequence;
    Inverter chargeTargetCheckNode;
    ActionNode setChargeTargetNode;



    ActionNode waitAction;
    ActionNode chargeAction;
    ActionNode chargeEndedCheckAction;

    Selector rootNode;
    Sequence playerNearbySequence;

    private void Start()
    {
        wanderTargetNullCheckInverter = new Inverter(new ActionNode(WanderTargetNullCheck));
        setWanderTargetNode = new ActionNode(SetNewWanderTarget);

        atWanderTargetCheckInverter = new ActionNode(AtWanderTargetCheck);
        resetWanderTargetNode = new ActionNode(ResetDestination);

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


        hostileTargetNullCheckNode = new ActionNode(HostileTargetNullCheck);
        hostileTargetChargeDistanceCheckNode = new Inverter(new ActionNode(ChargeDistanceCheck));
        moveTowardsPlayerAction = new ActionNode(MoveTowardsPlayer);

        chargeTargetCheckNode = new Inverter(new ActionNode(ChargeTargetCheck));
        setChargeTargetNode = new ActionNode(SetChargeTarget);
        waitAction = new ActionNode(Wait);
        chargeAction = new ActionNode(Charge);
        chargeEndedCheckAction = new ActionNode(ChargeEndedCheck);

        chargeTargetCheckSequence = new Sequence(new List<Node>
        {
            chargeTargetCheckNode,
            setChargeTargetNode,
            waitAction,
            chargeAction,
            chargeEndedCheckAction,
            waitAction,
        });

        chargeTargetDistanceCheckSequence = new Sequence(new List<Node>
        {
            hostileTargetChargeDistanceCheckNode,
            moveTowardsPlayerAction,
        });

        chargeSelector = new Selector(new List<Node>
        {
            chargeTargetDistanceCheckSequence,
            chargeTargetCheckSequence,
        });

        attackPlayerSequence = new Sequence(new List<Node>
        {
            hostileTargetNullCheckNode,
            chargeSelector,
        });

        rootNode = new Selector(new List<Node>
        {
            attackPlayerSequence,
            wanderSelector
        });

        //setTargetNode = new ActionNode(SetTarget);
        
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
    private NodeStates ChargeEndedCheck()
    {
        if (Vector3.Distance(transform.position, nav.destination) <= 0.5)
        {
            timer = 0;
            return NodeStates.SUCCESS;
        }

        return NodeStates.FAILURE;
    }

    private NodeStates Wait()
    {
        nav.isStopped = true;
       
        timer += Time.deltaTime;
        if (timer < chargeTime)
        {
            //transform.LookAt(navAgentDestination.transform);

            return NodeStates.FAILURE;
        }

        //timer = 0;
        return NodeStates.SUCCESS;
    }

    private NodeStates ChargeTargetCheck()
    {
        if (navAgentDestination.activeSelf == false)
        {
            return NodeStates.FAILURE;
        }

        return NodeStates.SUCCESS;
    }

    private NodeStates MoveTowardsPlayer()
    {
        nav.SetDestination(hostilityTarget.transform.position);
        nav.speed = walkSpeed;
        nav.enabled = true;
        nav.isStopped = false;
        return NodeStates.SUCCESS;
    }


    private NodeStates SetChargeTarget()
    {
        navAgentDestination.transform.position = hostilityTarget.transform.position + (transform.forward * 2);
        navAgentDestination.SetActive(true);
        return NodeStates.SUCCESS;
    }

    private NodeStates ClearNavAgentDestination()
    {
        navAgentDestination.SetActive(false);
        return NodeStates.SUCCESS;
    }

    private NodeStates MoveToTargetDestination()
    {
        nav.speed = walkSpeed;
        nav.enabled = true;
        nav.SetDestination(navAgentDestination.transform.position);
        return NodeStates.SUCCESS;
    }

    private NodeStates Charge()
    {
        nav.speed = runSpeed;
        nav.SetDestination(transform.forward * (minChargeDistance + 2));
        nav.isStopped = false;
        return NodeStates.SUCCESS;
    }

    private NodeStates WanderTargetNullCheck()
    {
        if (navAgentDestination == null)
        {
            navAgentDestination = new GameObject("Brian Wander Target");
            navAgentDestination.SetActive(false);
        }

        if (navAgentDestination.activeSelf == false)
        {
            return NodeStates.FAILURE;
        }
        else
        {
            return NodeStates.SUCCESS;
        }
    }

    private NodeStates ResetDestination()
    {
        navAgentDestination.SetActive(false);
        nav.enabled = false;
        return NodeStates.SUCCESS;
    }

    private NodeStates SetNewWanderTarget()
    {
        navAgentDestination.SetActive(true);
        navAgentDestination.transform.position = RandomNavSphere(transform.position, hostilityArea.radius, -1);
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


    private NodeStates HostileTargetNullCheck()
    {
        if (hostilityTarget)
        {
            Debug.Log("Player is in range");
            navAgentDestination.SetActive(false);
            return NodeStates.SUCCESS;
        }
        else
        {
            Debug.Log("Player is not in range");
            return NodeStates.FAILURE;
        }
    }

    private NodeStates ChargeDistanceCheck()
    {
        if (Vector3.Distance(transform.position, hostilityTarget.transform.position) <= minChargeDistance )
        {
            Debug.Log("Player is in charge range");
            return NodeStates.SUCCESS;
        }

        Debug.Log("Player is not in charge range");
        return NodeStates.FAILURE;
    }

    private NodeStates AtWanderTargetCheck()
    {
        if (Vector3.Distance(transform.position, navAgentDestination.transform.position) <= minDistanceToWanderTarget)
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

    private void OnDrawGizmos()
    {
        Color oldColor = Gizmos.color;
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, hostilityArea.radius);
        Gizmos.color = oldColor;



        if (navAgentDestination != null)
        {
            if (navAgentDestination.activeSelf)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(transform.position, navAgentDestination.transform.position);
                Gizmos.DrawWireSphere(navAgentDestination.transform.position, 1);
                Gizmos.color = oldColor;
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minChargeDistance);
        Gizmos.color = oldColor;

    }





}
