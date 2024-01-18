using UnityEngine;

public class EnemyVisibilityDetection : MonoBehaviour
{
    public float fieldOfView = 60f;
    public float fadeSpeed = 0.5f; // Speed of fading in/out
    public float proximityDistance = 5f;
    public string target = "";
    private bool isInitialized = false;
    public GameObject[] enemies;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        enemies = EnemyManager.Instance.GetEnemies();
        foreach (GameObject enemy in enemies)
        {
            Vector3 directionToEnemy = enemy.transform.position - transform.position;
            float distanceToEnemy = directionToEnemy.magnitude;
            float angleToEnemy = Vector3.Angle(directionToEnemy, transform.forward);

            if ((distanceToEnemy <= proximityDistance) && IsInLineOfSight(enemy) || (angleToEnemy < fieldOfView / 2 && IsInLineOfSight(enemy)))
            {
                FadeIn(enemy); // Fade in when in proximity or in sight
            }
            else
            {
                FadeOut(enemy); // Fade out when out of proximity and sight
            }
        }
    }

    private void Initialize()
    {
        if (isInitialized)
            return;
        
        EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null)
        {
            enemies = enemyManager.GetEnemies();
            Debug.Log(enemies);
            foreach (var enemy in enemies)
            {
                SetTransparency(enemy, 0f); // Initialize
            }
            isInitialized = true; // Mark as initialized
        }
        else
        {
            Debug.LogError("EnemyManager not found.");
        }
    }
    bool IsInLineOfSight(GameObject enemy)
    {
        bool isInSight = false;

        // Define several points on the enemy to test for visibility
        Transform enemyTransform = enemy.transform;
        var position = enemyTransform.position;
        Vector3[] targetPoints = new Vector3[]
        {
            position + enemyTransform.up * 0.8f, // Head
            position, // Chest
            position - enemyTransform.up * 0.5f, // Feet
            position + enemyTransform.right * 0.45f, // Right hand
            position - enemyTransform.right * 0.45f, // Left hand
            // ... Add more offsets for other body parts if needed
        };

        foreach (Vector3 targetPoint in targetPoints)
        {
            Vector3 directionToTarget = (targetPoint - transform.position).normalized;
            RaycastHit hit;
            
            if (Physics.Raycast(transform.position, directionToTarget, out hit, maxViewDistance))
            {
                if (hit.collider.gameObject == enemy)
                {
                    isInSight = true; // Enemy is in direct line of sight
                    break; // No need to check other points if one is already in sight
                }
            }
        }

        return isInSight; // Return true if any part of the enemy is in direct line of sight
    }

    private float maxViewDistance = 100f; // Example max view distance

    void FadeIn(GameObject enemy)
    {
        MeshRenderer renderer = enemy.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Color color = renderer.material.color;
            color.a = Mathf.Min(color.a + fadeSpeed * Time.deltaTime, 1f);
            renderer.material.color = color;

            if (color.a > 0f)
            {
                SetEnemyVisibility(enemy, true);
            }
        }
    }

    void FadeOut(GameObject enemy)
    {
        MeshRenderer renderer = enemy.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Color color = renderer.material.color;
            float newAlpha = Mathf.Max(color.a - fadeSpeed * Time.deltaTime, 0f);
            color.a = newAlpha;
            renderer.material.color = color;

            if (newAlpha == 0f)
            {
                SetEnemyVisibility(enemy, false);
            }
        }
    }

    void SetEnemyVisibility(GameObject enemy, bool isVisible)
    {
        MeshRenderer renderer = enemy.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = isVisible;
        }

        foreach (Transform child in enemy.transform)
        {
            child.gameObject.SetActive(isVisible);
        }
    }

    void SetTransparency(GameObject enemy, float alpha)
    {
        MeshRenderer renderer = enemy.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;
        }
    }
    void OnDrawGizmos()
    {
        // Draw FOV Cone
        Gizmos.color = Color.blue;
        DrawFOVCone();

        // Draw Proximity Circle
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, proximityDistance);
    }

    void DrawFOVCone()
    {
        float totalFOV = fieldOfView;
        float rayRange = maxViewDistance;
        float halfFOV = totalFOV / 2.0f;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;

        Gizmos.DrawRay(transform.position, leftRayDirection * rayRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * rayRange);

        // Additional rays for better visualization (optional)
        for (int i = 0; i < 10; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis(-halfFOV + (totalFOV / 10 * i), Vector3.up);
            Vector3 direction = rotation * transform.forward;
            Gizmos.DrawRay(transform.position, direction * rayRange);
        }
    }
}
