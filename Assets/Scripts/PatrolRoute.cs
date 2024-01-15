using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    public GameObject enemyPrefab; // Enemy prefab to instantiate
    public int numberOfEnemies; // Number of enemies to spawn

    public Color lineColor = Color.cyan; // Color of the Gizmos

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is not assigned.");
            return;
        }

        // Spawn enemies and assign them to this patrol route for (int i = 0; i < numberOfEnemies; i++)
        {
            for (int i = 0; i < numberOfEnemies; i++)
            {
                // Instantiate enemy at the position of the first checkpoint
                // or at the patrol route's position if there are no checkpoints.
                Vector3 spawnPosition = transform.childCount > 0 ? transform.GetChild(0).position : transform.position;

                Quaternion spawnRotation = Quaternion.identity; // Default rotation

                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);

                // Now we assign the patrol route to the enemy
                var enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.SetPatrolRoute(gameObject);
                }
                else
                {
                    Debug.LogError("Spawned enemy does not have an EnemyController component.");
                }
            }
        }
    }
    // GIZMOS //

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