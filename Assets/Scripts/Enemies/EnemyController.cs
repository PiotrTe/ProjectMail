using System;
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
    public float viewAngle = 115f; // Angle of view
    public float rotationSpeed = 3f; // Speed of rotation for looking in specified direction
    private Transform player; // Reference to the player
    private Vector3 lastSeenPosition;
    private bool reachedLastSeenPosition;
    private bool canMove;
    private float idleTimer;
    private float investigateTime;
    private float cooldownTime = 2.0f;
    private float cooldownTimer;
    public float alertness = 0.0f;
    public float maxAlertness = 100.0f; // Threshold for switching to investigation
    public float alertnessIncreaseRate = 100.0f; // Rate at which alertness increases per second
    public float sightAlertnessIncreaseRate = 100.0f; // Rate at which alertness increases per second
    public float alertnessDecreaseRate = 20.0f; // Rate at which alertness increases per second
    private float idleRotationAngle = 90f; // Maximum angle to rotate while idling


    

    public enum State { Patrolling, Investigating, Chasing, Idling }


    private State currentState = State.Patrolling;

    private void Awake()
    {
        EnemyManager.Instance.AddEnemy(gameObject);
    }

    void Start()
    {
        leftRotation = Quaternion.Euler(0, -idleRotationAngle, 0);
        rightRotation = Quaternion.Euler(0, idleRotationAngle, 0);
        targetRotation = leftRotation;
        
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        InitializeCheckpoints();
        MoveToNextCheckpoint();
    }
    void OnDestroy()
    {
        EnemyManager.Instance.RemoveEnemy(gameObject);
    }
    void Update()
    {
        UpdateMovementAbility();
        if (HasLineOfSight())
        {
            SeePlayer(player.position);
        }
        
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else if (alertness > 0)
        {
            alertness -= alertnessDecreaseRate * Time.deltaTime;
        }

        // Ensure alertness doesn't go below 0
        if (alertness < 0) { alertness = 0; }

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
            return hit.transform == player; // Return true if player is in direct line of sight
        }

        return false; // Return false if an obstacle is blocking the view
    }
    public void HearPlayer(Vector3 playerPosition)
    {  
        player.GetComponent<PlayerController>().TakeDamage(5);
        RotateTowards(playerPosition);
        alertness += alertnessIncreaseRate * Time.deltaTime;
        cooldownTimer = cooldownTime; // Reset cooldown timer
        if (alertness >= maxAlertness)
        {
            lastSeenPosition = playerPosition; // Update last seen position
            currentState = State.Investigating;
            reachedLastSeenPosition = false;
            agent.SetDestination(lastSeenPosition);
            ResetAlertness(); // Optionally reset alertness
        }
    }
    public void SeePlayer(Vector3 playerPosition)
    {
        
        player.GetComponent<PlayerController>().TakeDamage(25);
        RotateTowards(playerPosition);
        alertness += sightAlertnessIncreaseRate * Time.deltaTime;
        cooldownTimer = cooldownTime; // Reset cooldown timer

        if (alertness >= maxAlertness)
        {
            alertness = maxAlertness;
            lastSeenPosition = playerPosition; // Update last seen position
            currentState = State.Chasing;
            reachedLastSeenPosition = false;
            agent.SetDestination(lastSeenPosition);
            ResetAlertness(); // Optionally reset alertness
        }
    }
    private void ResetAlertness()
    {
        alertness = 0.0f;
    }
    private void RotateTowards(Vector3 direction)
    {
        if (player == null) return; 
        
        Vector3 lookDirection = direction - transform.position;
        lookDirection.y = 0; // Optional: Horizontal rotation only

        // Calculate the rotation needed to look at the player
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        // Apply the rotation (smoothly or directly)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    // STATES //
    private void UpdateMovementAbility()
    {
        // If alertness is rising and the enemy is not chasing, it should not move.
        if (alertness > 0 && currentState == State.Patrolling)
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }

        // Enable or disable the agent's movement based on canMove.
        agent.isStopped = !canMove;
    }
    public void HandleChasing()
    {
        canMove = true;
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
        canMove = true;
        if (!reachedLastSeenPosition)
        {
            // Move to the last known position of the player.
            if (Vector3.Distance(transform.position, lastSeenPosition) < agent.stoppingDistance)
            {
                // Arrived at the last known position.
                reachedLastSeenPosition = true;
                currentState = State.Idling;
                idleTimer = 10.0f;
            }
        }
    }
    
    private Quaternion leftRotation;
    private Quaternion rightRotation;
    private Quaternion targetRotation;
    void HandleIdling()
    {
        canMove = false;

        if (idleTimer <= 0f)
        {
            currentState = State.Patrolling;
            idleTimer = 0;
        }
        else
        {
            idleTimer -= Time.deltaTime;

            // Check if the rotation has reached the target, and switch the target
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                targetRotation = (targetRotation == leftRotation) ? rightRotation : leftRotation;
            }

            // Perform smooth rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, (rotationSpeed / 3) * Time.deltaTime);
        }
    }
    void HandlePatrolling()
    {
        canMove = true;
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
        Handles.Label(transform.position + Vector3.up * 2.0f, (currentState.ToString() + idleTimer + alertness), style);
    }
    
}
