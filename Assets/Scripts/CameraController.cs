using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    private Vector3 offset;

    public float shakeStrength = 0.0f;
    public float shakeDuration = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position + offset;
        transform.LookAt(player.transform.position);

    }

    public void Shake(float shakeStrength, float shakeDuration)
    {

    }
}
