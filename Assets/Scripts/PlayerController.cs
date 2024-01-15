using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    
    [SerializeField] private float speed = 200f;
    Rigidbody rbody;
    public float health = 200.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane characterPlane = new Plane(Vector3.up, transform.position);
        float intersectionDistance;

        if (characterPlane.Raycast(cursorRay, out intersectionDistance))
        {
            Vector3 intersectionPoint = cursorRay.origin + cursorRay.direction * intersectionDistance;
            transform.LookAt(intersectionPoint);
        }


    }
    void FixedUpdate()
    {

        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), 0 ,Input.GetAxisRaw("Vertical"));

        move.Normalize();
        rbody.AddForce(move * speed * Time.deltaTime, ForceMode.Impulse);
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
    }

}
