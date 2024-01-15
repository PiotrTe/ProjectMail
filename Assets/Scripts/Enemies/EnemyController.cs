using System.Collections;
using UnityEngine;
using UnityEngine.AI; // For NavMeshAgent

public class EnemyController : MonoBehaviour
{
    public GameObject patrolRoute; // Reference to the patrol route GameObject
    private Transform[] checkpoints; // Array of checkpoints
    private int currentCheckpointIndex = 0;
    private NavMeshAgent agent;
    public float investigateTime = 5f; // Time spent in investigation
    public float viewRadius = 10f; // Detection range
    public float viewAngle = 45f; // Angle of view
    public Transform player; // Reference to the player
    private bool shouldMoveToNextCheckpoint = true; // New flag
    private Vector3 lastSeenPosition;
    private bool reachedLastSeenPosition = false;
    private float lookAroundTime = 2.5f; // Time to look around in one direction
    private bool isLookingRight = true; // Direction of looking around

    

    public enum State { Patrolling, Investigating, Idling }

    private State currentState = State.Patrolling;

    void Start()
    {
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
        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrolling();
                break;
            case State.Investigating:
                HandleInvestigating();
                break;
            case State.Idling:
                HandleIdling();
                break;
        }

        if (HasLineOfSight())
        {
            OnPlayerDetected(player.position);
        }
    }
    private void InitializeCheckpoints()
    {
        if (patrolRoute != null)
        {
            int children = patrolRoute.transform.childCount;
            checkpoints = new Transform[children];
            for (int i = 0; i < children; i++)
            {
                checkpoints[i] = patrolRoute.transform.GetChild(i);
            }
        }
    }
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
        if (!IsPlayerInView())
            return false;

        RaycastHit hit;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, viewRadius))
        {
            if (hit.transform == player)
            {
                return true; // Player is in direct line of sight
            }
        }
        return false; // Obstacle is blocking the view
    }
    void MoveToNextCheckpoint()
    {
        if (checkpoints == null || checkpoints.Length == 0)
            return;

        Transform nextCheckpoint = checkpoints[currentCheckpointIndex];
        agent.SetDestination(nextCheckpoint.position);
    }
    private float idleTimer = 5f; // Time spent in idle state
    private float rotationSpeed = 30f; // Speed at which the enemy rotates while idling
    void HandleInvestigating()
    {
        if (!reachedLastSeenPosition)
        {
            if (Vector3.Distance(transform.position, lastSeenPosition) < agent.stoppingDistance)
            {
                reachedLastSeenPosition = true;
                lookAroundTime = 2.5f; // Reset look around time
            }
        }
        else
        {
            LookAround();
        }
    }
    void HandleIdling()
    {
        // Transition back to patrolling after a certain time
        if (idleTimer <= 0f)
        {
            currentState = State.Patrolling;
            MoveToNextCheckpoint();
            idleTimer = 5f; // Reset idle timer
        }
        else
        {
            idleTimer -= Time.deltaTime;
        }
    }
    void HandlePatrolling()
    {
        if (checkpoints == null || checkpoints.Length == 0)
            return;

        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance)
        {
            if (shouldMoveToNextCheckpoint)
            {
                CheckCheckpointBehavior(checkpoints[currentCheckpointIndex]);
                currentCheckpointIndex = (currentCheckpointIndex + 1) % checkpoints.Length;
                if (shouldMoveToNextCheckpoint)
                {
                    MoveToNextCheckpoint();
                }
            }
        }
    }

    void CheckCheckpointBehavior(Transform checkpoint)
    {
        var checkpointBehavior = checkpoint.GetComponent<CheckpointBehavior>();
        if (checkpointBehavior != null)
        {
            // Apply the behavior from the checkpoint
            ApplyCheckpointBehavior(checkpointBehavior);
        }
    }
    void ApplyCheckpointBehavior(CheckpointBehavior behavior)
    {
        // Set the new state and duration
        currentState = behavior.newState;

        switch (behavior.newState)
        {
            case State.Patrolling:
                // Enable moving to the next checkpoint
                shouldMoveToNextCheckpoint = true;
                break;
            case State.Investigating:
                // Apply investigating specific behavior
                investigateTime = behavior.stateDuration;
                shouldMoveToNextCheckpoint = false;
                break;
            case State.Idling:
                // Apply idling specific behavior
                idleTimer = behavior.stateDuration;
                shouldMoveToNextCheckpoint = false;
                StartCoroutine(ResetMoveToNextCheckpointAfterIdle(idleTimer));
                break;
        }

        // Play the animation
        // Assuming you have an Animator component
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(behavior.animationName);
        }
    }

    // Call this method when the player is detected
    public void OnPlayerDetected(Vector3 lastKnownPosition)
    {
        currentState = State.Investigating;
        lastSeenPosition = lastKnownPosition;
        reachedLastSeenPosition = false;
        agent.SetDestination(lastSeenPosition);
    }
    private void InitializePatrolPoints()
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
        InitializePatrolPoints();
    }
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
    }
    void LookAround()
    {
        if (lookAroundTime > 0f)
        {
            // Rotate in the current direction
            float rotationAngle = rotationSpeed * Time.deltaTime * (isLookingRight ? 1 : -1);
            transform.Rotate(0, rotationAngle, 0);
            lookAroundTime -= Time.deltaTime;
        }
        else
        {
            isLookingRight = !isLookingRight; // Switch direction
            lookAroundTime = 2.5f; // Reset look around time

            if (!isLookingRight) // After looking both ways, return to patrolling
            {
                currentState = State.Patrolling;
                MoveToNextCheckpoint();
            }
        }
    }
    IEnumerator ResetMoveToNextCheckpointAfterIdle(float delay)
    {
        yield return new WaitForSeconds(delay);
        shouldMoveToNextCheckpoint = true;
        MoveToNextCheckpoint();
    }
    
}
