using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    public GameObject[] enemies; // Array to hold assigned enemies

    void Start()
    {
        // Assign this route to each enemy
        foreach (var enemy in enemies)
        {
            var enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.SetPatrolRoute(gameObject);
            }
        }
    }

    public Color lineColor = Color.cyan; // Color of the Gizmos

    void OnDrawGizmos()
    {
        int childCount = transform.childCount;

        if (childCount > 1)
        {
            Gizmos.color = lineColor;

            // Draw lines between each child waypoint
            for (int i = 0; i < childCount; i++)
            {
                Vector3 currentPoint = transform.GetChild(i).position;
                Vector3 nextPoint = transform.GetChild((i + 1) % childCount).position;

                Gizmos.DrawLine(currentPoint, nextPoint);
            }
        }

        // Draw a wireframe around the parent object
        Gizmos.color = lineColor;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}