using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
public class PlayerController : MonoBehaviour
{
    
    [SerializeField] private float speed = 200f;
    Rigidbody rbody;
    public float health = 200.0f;
    public PostProcessVolume volume;
    private Vignette vignette;
    Animator m_Animator;
    
    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
    }
    void Update()
    {

        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane characterPlane = new Plane(Vector3.up, transform.position);
        float intersectionDistance;
        UpdateVignetteEffect();
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

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            m_Animator.SetBool("isWalking", true);
        }
        else
        {
            m_Animator.SetBool("isWalking", false);
        }
    }
    
    
    private void UpdateVignetteEffect()
    {
        if (vignette != null)
        {
            float healthPercentage = health / 200.0f; // Assuming max health is 200
            vignette.intensity.value = 1.0f - healthPercentage; // Increase intensity as health decreases
        }
    }
    
    public void TakeDamage(float damage, float maxDamage)
    {
        if (health >= maxDamage)
        {
            health -= damage;
        }
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
    }

}
