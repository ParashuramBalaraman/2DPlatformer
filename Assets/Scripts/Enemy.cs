using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [SerializeField] float m_JumpForce = 10.0f;	                        // Amount of force added when the player jumps.
	[SerializeField] float m_RollForce = 7.0f;							// Amount of force added when the player rolls.
	[SerializeField] float m_Speed = 10.0f;										
	[SerializeField] private Collider2D m_RollDisableCollider;		    // A collider that will be disabled when rolling.
    public Animator             m_Animator;
    private Rigidbody2D         m_Body2d;
    private Sensor_HeroKnight   m_GroundSensor;
    private Sensor_HeroKnight   m_WallSensorR1;
    private Sensor_HeroKnight   m_WallSensorR2;
    private Sensor_HeroKnight   m_WallSensorL1;
    private Sensor_HeroKnight   m_WallSensorL2;
    private bool                m_Grounded = false;                     // If player is grounded.                      
    public bool                 m_Rolling = false;					    // If player is rolling.
    private int                 m_FacingDirection = 1;					// What direction player is facing. 
    private int                 m_CurrentAttack = 0;
    private float               m_TimeSinceAttack = 0.0f;
    private float               m_DelayToIdle = 0.0f;
    private float               m_RollDuration = 8.0f / 14.0f;			// Length of a rolling manoveur. 
    private float               m_RollCurrentTime;						// Duration of current roll.


    public Transform attackPoint;
    public LayerMask playerLayer;
    public float attackRange = 0.5f;
    public int attackDamage = 25;
    public int maxHealth = 100;
    int currentHealth; 


	// Start is used for initialization
	void Start() {
		m_Animator = GetComponent<Animator>();
        m_Body2d = GetComponent<Rigidbody2D>();
        m_GroundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_WallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_WallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_WallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_WallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        
        currentHealth = maxHealth;
	}

	// Update is called once per frame
	void Update() {

		// Increase timer that controls attack combo
        m_TimeSinceAttack += Time.deltaTime;

		// Increase timer of current roll duration
		if (m_Rolling){
			m_RollCurrentTime += Time.deltaTime;

			// Disbale the collider of the player while rolling
			if (m_RollDisableCollider != null){
				m_RollDisableCollider.enabled = false;
			}
		}
		// If current roll duration exceeds duration disable roll
		if(m_RollCurrentTime > m_RollDuration){
			m_Rolling = false;

			// Enable the collider of the player while not rolling
			if (m_RollDisableCollider != null){
				m_RollDisableCollider.enabled = true;
			}
		}

		// Check whether player just landed 
		if(!m_Grounded && m_GroundSensor.State()){
			m_Grounded = true;
			m_Animator.SetBool("Grounded", m_Grounded);
		}
		// Check whether player has just started falling
		if(m_Grounded && !m_GroundSensor.State()){
			m_Grounded = false;
			m_Animator.SetBool("Grounded", m_Grounded);
		}

		// -- Handle input and movement --
		float inputX = Input.GetAxis("P2Horizontal");
		// Swap direction of sprite depending on walking direction
		if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_FacingDirection = 1;
        } else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_FacingDirection = -1;
        }

		// Movement
		if (!m_Rolling){
			m_Body2d.velocity = new Vector2(inputX * m_Speed, m_Body2d.velocity.y);
		}

		// Set AirSpeed in animator
        m_Animator.SetFloat("AirSpeedY", m_Body2d.velocity.y);

        // -- Handle Animations --
		// Attack animation
		if (Input.GetMouseButtonDown(0) && m_TimeSinceAttack > 0.25f && !m_Rolling){
            m_CurrentAttack++;
			
			// Loop back to first attack animation after the third
			if (m_CurrentAttack > 3){
				m_CurrentAttack = 1;
			}

			// If too much time passes in between attacks reset back to first attack animation
			if (m_TimeSinceAttack > 1f){
				m_CurrentAttack = 1;
			}

			// Call the relevant attack animation (out of "Attack1", "Attack2", "Attack3")
			m_Animator.SetTrigger("Attack" + m_CurrentAttack);
            Attack();

			// Reset timesince attack timer
			m_TimeSinceAttack = 0f;
		}

		// Roll animation
		else if (Input.GetKeyDown("down") && !m_Rolling){
			m_Rolling = true;
			m_Animator.SetTrigger("Roll");
			m_Body2d.velocity = new Vector2(m_FacingDirection * m_RollForce, m_Body2d.velocity.y);
		}

		// Jump animation
		else if (Input.GetKeyDown("up") && m_Grounded && !m_Rolling){
			m_Animator.SetTrigger("Jump");
            m_Grounded = false;
            m_Animator.SetBool("Grounded", m_Grounded);
            m_Body2d.velocity = new Vector2(m_Body2d.velocity.x, m_JumpForce);
            m_GroundSensor.Disable(0.2f);
		}

		// Run animation
		else if (Mathf.Abs(inputX) > Mathf.Epsilon){
			// Reset timer
			m_DelayToIdle = 0.05f;
			m_Animator.SetInteger("AnimationState", 1);
		}

		// Idle Animation
		else{
			// Prevents flickering when transitioning to idle
			m_DelayToIdle -= Time.deltaTime;
			if(m_DelayToIdle < 0){
				m_Animator.SetInteger("AnimationState", 0);
			}
		}
	}

    void Attack()
    {
        // Detect enemies in range
        Collider2D player = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);
        // Damage enemies in range
        if (player.GetComponent<Player>().m_Rolling == false){
            player.GetComponent<Player>().TakeDamage(attackDamage);
        }
    }

    void OnDrawGizmosSelected() {
        if (attackPoint == null){
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void TakeDamage(int damage){
        m_Animator.SetTrigger("Hurt");
        currentHealth -= damage;

        if (currentHealth <= 0){
            Die();
        }
    }

    public void Die(){
        m_Animator.SetBool("Dead", true);
        m_Body2d.constraints = RigidbodyConstraints2D.FreezeAll;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
		// Change Scene after 1 second
		StartCoroutine(ChangeScene("TreePlayer1Win"));

    }

	public IEnumerator ChangeScene(string SceneToChangeTo){
		// Delays the transition between scenes by a second, so that the death animation will play first
		yield return new WaitForSeconds(1);
		SceneManager.LoadScene(SceneToChangeTo);
	}
    
}
