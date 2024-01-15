using UnityEngine;

public class EnemyVisibilityDetection : MonoBehaviour
{
    public float maxViewDistance = 15f;
    public float fieldOfView = 60f;
    public float fadeSpeed = 0.5f; // Speed of fading in/out
    public float proximityDistance = 5f;
    public string target = "";

    private GameObject[] enemies;

    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag(target);
        foreach (var enemy in enemies)
        {
            SetTransparency(enemy, 0f); // Initialize
        }
    }
    void Update()
    {
        foreach (GameObject enemy in enemies)
        {
            Vector3 directionToEnemy = enemy.transform.position - transform.position;
            float distanceToEnemy = directionToEnemy.magnitude;
            float angleToEnemy = Vector3.Angle(directionToEnemy, transform.forward);

            if ((distanceToEnemy <= proximityDistance) && IsInLineOfSight(enemy, directionToEnemy) || (angleToEnemy < fieldOfView / 2 && IsInLineOfSight(enemy, directionToEnemy)))
            {
                FadeIn(enemy); // Fade in when in proximity or in sight
            }
            else
            {
                FadeOut(enemy); // Fade out when out of proximity and sight
            }
        }
    }

    bool IsInLineOfSight(GameObject enemy, Vector3 directionToEnemy)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToEnemy.normalized, out hit, maxViewDistance))
        {
            if (hit.collider.gameObject == enemy)
            {
                return true;
            }
            else if (hit.collider.CompareTag(target))
            {
                Collider hitEnemyCollider = hit.collider;
                hitEnemyCollider.enabled = false;

                bool lineOfSight = Physics.Raycast(transform.position, directionToEnemy.normalized, out hit, maxViewDistance) && hit.collider.gameObject == enemy;

                hitEnemyCollider.enabled = true;

                return lineOfSight;
            }
        }
        return false;
    }

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
