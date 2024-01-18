using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    
    private float speed = 100f;
    private float sneakSpeed = 40f;
    private float soundRange = 10f;
    private float stressDecayRate = 10.0f; // Rate at which stress decreases per second
    private float stressCooldownDuration = 5.0f; // Cooldown duration before stress starts to decrease
    private float stressCooldownTimer = 0.0f;
    public Volume globalVolume; // Assign this in the Inspector
    private Vignette vignette;
    public float stress = 0.0f;
    public float maxStress = 200.0f;
    Rigidbody rbody;
    Animator m_Animator;
    private bool isSneaking;
    public AudioSource footSteps;
    
    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();

        m_Animator = GetComponent<Animator>();

        if (globalVolume.profile.TryGet<Vignette>(out vignette))
        {
            // Vignette component found
        }
        else
        {
            Debug.LogWarning("Vignette component not found in the global volume");
        }
    }
    void Update()
    {
        UpdateVignetteEffect();
        Ray cursorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane characterPlane = new Plane(Vector3.up, transform.position);
        float intersectionDistance;
        UpdateVignetteEffect();
        if (stress >= maxStress)
        {
            Die();
        }
        if (characterPlane.Raycast(cursorRay, out intersectionDistance))
        {
            Vector3 intersectionPoint = cursorRay.origin + cursorRay.direction * intersectionDistance;
            transform.LookAt(intersectionPoint);
        }
        if (stressCooldownTimer <= 0.0f)
        {
            DecreaseStressOverTime();
        }
        else
        {
            stressCooldownTimer -= Time.deltaTime;
        }
    }
    void FixedUpdate()
    {
        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), 0 ,Input.GetAxisRaw("Vertical"));
        move.Normalize();
        isSneaking = Input.GetKey(KeyCode.LeftControl);

        float currentForce = isSneaking ? sneakSpeed : speed;
        rbody.AddForce(move * (currentForce * Time.deltaTime), ForceMode.Impulse);

        // Animation
        m_Animator.SetBool("isWalking", move.magnitude > 0);
        
        if (move.magnitude > 0)
        {
            footSteps.enabled = true;
        }
        else
        {
            footSteps.enabled = false;
        }

        // Update sound range based on velocity
        UpdateSoundRange(rbody.velocity.magnitude);
        CheckForEnemies();
    }
    void Die()
    {
        speed = 0f;
        Invoke("LoadDeathScene", 2f);
    }
    void LoadDeathScene()
    {
        SceneManagementController.Instance.LoadScene("DeathScene");
    }
    private void UpdateSoundRange(float velocityMagnitude)
    {
        // Example: Map the velocity magnitude to a sound range between 5 and 10
        // Adjust these values according to your game's needs
        float minVelocity = 0f;  // Minimum velocity to start making sound
        float maxVelocity = 10f; // Velocity at which sound range is at maximum

        // Normalize the velocity into a 0-1 range and then interpolate sound range
        float normalizedVelocity = Mathf.Clamp((velocityMagnitude - minVelocity) / (maxVelocity - minVelocity), 0f, 1f);
        soundRange = Mathf.Lerp(0f, isSneaking ? 5f : 10f, normalizedVelocity);
    }
    private void CheckForEnemies()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= soundRange)
            {
                RaycastHit hit;
                Vector3 directionToEnemy = enemy.transform.position - transform.position;

                // Check if there's an obstacle between player and enemy
                if (Physics.Raycast(transform.position, directionToEnemy, out hit, soundRange))
                {
                    if (hit.collider.gameObject == enemy)
                    {
                        // Enemy is in sound range and has direct line of sight
                        AlertEnemy(enemy);
                    }
                    else
                    {
                        // Check if enemy is within reduced range due to obstacle
                        if (distance <= soundRange / 2)
                        {
                            AlertEnemy(enemy);
                        }
                    }
                }
            }
        }
    }
    private void AlertEnemy(GameObject enemy)
    {
        enemy.GetComponent<EnemyController>().HearPlayer(transform.position);
        Debug.Log("Enemy Alerted: " + enemy.name);
    }
    
    
    private void UpdateVignetteEffect()
    {
        if (vignette != null)
        {
            float stressPercentage = Mathf.Clamp01(stress / maxStress);

            vignette.intensity.value = stressPercentage;

            float halfMaxStress = maxStress / 2.0f;
            float colorChangeStart = Mathf.Max(stress - halfMaxStress, 0) / halfMaxStress;
        
            vignette.color.value = Color.Lerp(Color.black, Color.red, colorChangeStart);
        }
    }
    private void DecreaseStressOverTime()
    {
        if (stress > 0.0f)
        {
            float stressDecay = stressDecayRate * Time.deltaTime;
            stress = Mathf.Max(stress - stressDecay, 0.0f);
        }
    }
    public void TakeDamage(float damage)
    {
        stress += damage * Time.deltaTime;
        stress = Mathf.Clamp(stress, 0, maxStress);

        // Reset the cooldown timer when taking damage
        stressCooldownTimer = stressCooldownDuration;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, soundRange);
    }
}
