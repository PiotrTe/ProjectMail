// /path/to/CheckpointBehavior.cs
using UnityEngine;

public class CheckpointBehavior : MonoBehaviour
{
    public string animationName;
    public EnemyController.State newState;
    public float stateDuration;
    public bool faceDirectionAtCheckpoint = false;
    public float angleToFace; // Angle to face at checkpoint
    void OnDrawGizmos()
    {
        if (faceDirectionAtCheckpoint)
        {
            // Draw a line indicating the facing direction
            Gizmos.color = Color.blue; // You can choose any color
            Vector3 direction = Quaternion.Euler(0, angleToFace, 0) * Vector3.forward;
            Gizmos.DrawRay(transform.position, direction * 2); // Adjust the length as needed
        }
    }
}