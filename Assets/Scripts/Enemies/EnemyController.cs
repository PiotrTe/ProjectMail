using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI; // For NavMeshAgent

public class EnemyController : MonoBehaviour
{
    public GameObject patrolRoute; // Reference to the patrol route GameObject
    private Transform[] checkpoints; // Array of checkpoints
    private int currentCheckpointIndex = 0;
    private NavMeshAgent agent;
    public float viewRadius = 10f; // Detection range
    public float viewAngle = 45f; // Angle of view
    private Transform player; // Reference to the player
    private Vector3 lastSeenPosition;
    private bool reachedLastSeenPosition;
    private float idleTimer;
    private float investigateTime;

    

    public enum State { Patrolling, Investigating, Chasing, Idling }


    private State currentState = State.Patrolling;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        EnemyManager.Instance.AddEnemy(gameObject);
        InitializeCheckpoints();
        MoveToNextCheckpoint();
    }
    void OnDestroy()
    {
        EnemyManager.Instance.RemoveEnemy(gameObject);
    }
    void Update()
    {
        if (idleTimer > 0)
            HandleIdling();
        
        switch (currentState)
        {
            case State.Idling:
                if (idleTimer > 0)
                    HandleIdling();
                else
                    currentState = State.Patrolling;
                break;
            case State.Patrolling:
                HandlePatrolling();
                break;
            case State.Investigating:
                HandleInvestigating();
                break;
            case State.Chasing:
                HandleChasing();
                break;
        }
    }
    
    // CHECKPOINTS //
    private void MoveToNextCheckpoint()
    {
        agent.SetDestination(checkpoints[currentCheckpointIndex].position);
    }
    private void InitializeCheckpoints()
    {
        // Initialize patrol points from patrolRoute children
        if (patrolRoute != null)
        {
            checkpoints = new Transform[patrolRoute.transform.childCount];
            for (int i = 0; i < checkpoints.Length; i++)
            {
                checkpoints[i] = patrolRoute.transform.GetChild(i);
            }
        }
    }
    public void SetPatrolRoute(GameObject route)
    {
        patrolRoute = route;
        InitializeCheckpoints();
    }
    
    
    // VISION //
    bool IsPlayerInView()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= viewRadius)
            {
                return true;
            }
        }
        return false;
    }
    bool HasLineOfSight()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (!IsPlayerInView())
            return false;

        int layerMask = ~(1 << LayerMask.NameToLayer("Enemy")); // Exclude the "Enemy" layer
        RaycastHit hit;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, viewRadius, layerMask))
        {
            if (hit.transform == player)
            {
                currentState = State.Chasing;
                lastSeenPosition = player.position;
                return true; // Player is in direct line of sight
            }
        }

        return false; // Obstacle is blocking the view
    }

    // STATES //
    void HandleChasing()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);    
            // If the enemy has line of sight to the player, keep chasing.
            if (HasLineOfSight())
            {
                // If still in sight, just update the last seen position.
                lastSeenPosition = player.position;
            }
            else
            {
                // If the enemy loses line of sight, start investigating the last known position.
                currentState = State.Investigating;
                reachedLastSeenPosition = false;
                agent.SetDestination(lastSeenPosition);
            }
        }
    }
    void HandleInvestigating()
    {
        if (!reachedLastSeenPosition)
        {
            // Move to the last known position of the player.
            if (Vector3.Distance(transform.position, lastSeenPosition) < agent.stoppingDistance)
            {
                // Arrived at the last known position.
                reachedLastSeenPosition = true;
                currentState = State.Idling;
                idleTimer += 10.0f;
            }
        }
    }
    void HandleIdling()
    {
        if (idleTimer <= 0f)
        {
            currentState = State.Patrolling;
        }
        else
        {
            idleTimer -= Time.deltaTime;
        }
    }
    void HandlePatrolling()
    {
        if (checkpoints == null || checkpoints.Length == 0) 
        {
            Debug.LogError("No checkpoints set for patrol route.");
            return;
        }

        // Check if we've reached the current checkpoint
        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance) 
        {
            currentCheckpointIndex = (currentCheckpointIndex + 1) % checkpoints.Length;
            MoveToNextCheckpoint();
        }
    }

    
    // GIZMOS //
    void OnDrawGizmos()
    {
        // Draw the field of view cone
        Gizmos.color = HasLineOfSight() ? Color.red : Color.yellow; // Red if player is detected, yellow otherwise
        Transform transform2;
        Vector3 fovLine1 = Quaternion.AngleAxis(viewAngle / 2, transform.up) * (transform2 = transform).forward * viewRadius;
        Transform transform1;
        Vector3 fovLine2 = Quaternion.AngleAxis(-viewAngle / 2, transform2.up) * (transform1 = transform).forward * viewRadius;

        Gizmos.DrawRay(transform1.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);

        if (currentState == State.Investigating)
        {
            // Draw a line to the investigation point (if any)
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, agent.destination);
        }

        // Show the current state as a text label above the enemy
        Gizmos.color = Color.white;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        Handles.Label(transform.position + Vector3.up * 2.0f, (currentState.ToString() + idleTimer), style);
    }
    
}
