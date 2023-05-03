using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AIController2D : MonoBehaviour, IDamagable
{
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float speed;
    [SerializeField] float jumpHeight;
    [SerializeField, Range(1, 5)] float fallRateMultiplier;
    [Header("Ground")]
    [SerializeField] Transform groundTransform;
    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] float groundRadius;
    [Header("AI")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float rayDistance = 1;
    [SerializeField] string enemyTag;
    [SerializeField] LayerMask raycastLayerMask;
    
    [Header("Health")]
    public float health = 100;

    Rigidbody2D rb;

    Vector2 velocity = Vector2.zero;
    bool faceRight = true;
    float groundAngle = 0;
    Transform targetWaypoint = null;
    GameObject enemy = null;

    enum State
    {
        IDLE,
        PATROL,
        CHASE,
        ATTACK
    }

    State state = State.IDLE;
    float stateTimer = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {

        //Update AI
        CheckEnemySeen();

        Vector2 direction = Vector2.zero;
        switch (state)
        {
            case State.IDLE:
                //if enemy is seen, chase
                if (enemy != null) state = State.CHASE;
                stateTimer -= Time.deltaTime; //countdown to move to next waypoint
                if (stateTimer <= 0)
                {
                    SetNewWayPointTarget();
                    state = State.PATROL;
                }
                break;
            case State.PATROL:
                {
                    //if enemy is seen, chase
                    if (enemy != null) state = State.CHASE;

                    //else ove towards waypoints
                    direction.x = Mathf.Sin(targetWaypoint.position.x - transform.position.x);
                    float dx = Mathf.Abs(targetWaypoint.position.x - transform.position.x);
                    if (dx <= 0.25f)
                    {
                        direction.x = 0;
                        stateTimer = 1;
                        state = State.IDLE;
                    }
                }
                break;
            case State.CHASE:
                {
                    //If the player is not seen, set the state to Idle
                    if (enemy == null)
                    {
                        stateTimer = 1;
                        state = State.IDLE;
                        break;
                    }
                    //if distance is less than or equal to 1 unit, attack
                    float dx = Mathf.Abs(enemy.transform.position.x - transform.position.x);
                    if (dx <= 1f)
                    {
                        animator.SetTrigger("Attack"); //plays animation
                        state = State.ATTACK;
                    }
                    else
                    {
                        //move towards player
                        direction.x = Mathf.Sign(enemy.transform.position.x - transform.position.x);
                    }
                }
                break;
            case State.ATTACK:
                {
                    //once animation ends, go back to chase state
                    if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0))
                    {
                        state = State.CHASE;
                    }
                }
                break;
            default:
                break;
        }


        // check if the character is on the ground
        bool onGround = UpdateGroundCheck();

        // get direction input
        //direction.x = Input.GetAxis("Horizontal");
        
        velocity.x = direction.x * speed;

        // set velocity
        if (onGround)
        {
            if (velocity.y < 0) velocity.y = 0;
            //if (Input.GetButtonDown("Jump"))
            //{
            //    velocity.y += Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
            //}
        }
        
        // adjust gravity for jump
        float gravityMultiplier = 1;
        if (!onGround && velocity.y < 0) gravityMultiplier = fallRateMultiplier;

        velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;

        //moves AI using rigidbody
        rb.velocity = velocity;

        //flip character to face direction of movement
        if (velocity.x > 0 && !faceRight) Flip();
        if (velocity.x < 0 && faceRight) Flip();

        //update animator
        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        animator.SetBool("Fall", !onGround && velocity.y < -0.1f);
    }

    private bool UpdateGroundCheck() 
    {
        //makes a collider circle using passed in values
        Collider2D collider = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask);
        if (collider != null) 
        {
            RaycastHit2D raycastHit = Physics2D.Raycast(groundTransform.position, velocity, rayDistance, raycastLayerMask);
            if (raycastHit.collider != null)
            {
                groundAngle = Vector2.SignedAngle(Vector2.up, raycastHit.normal);
                Debug.DrawRay(raycastHit.point, raycastHit.normal, Color.red);
            }
        }
        return (collider != null);
    }

    private void Flip() 
    {
        faceRight = !faceRight;

        spriteRenderer.flipX = faceRight;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(groundTransform.position, groundRadius);
    }

    private void SetNewWayPointTarget()
    {
        Transform waypoint = null;
        do
        {
            waypoint = waypoints[Random.Range(0, waypoints.Length)];
        } while (waypoint == targetWaypoint);

        targetWaypoint = waypoint;
    }

    private void CheckEnemySeen()
    {
        //sets enemy to null (obviously)
        enemy = null;
        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, ((faceRight) ? Vector2.right : Vector2.left), rayDistance, raycastLayerMask);
        if (raycastHit.collider != null && raycastHit.collider.gameObject.CompareTag(enemyTag))
        {
            enemy = raycastHit.collider.gameObject;
            Debug.DrawRay(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * rayDistance, Color.red);
        }
    }

    public void Damage(int damage)
    {
        health -= damage;
        print(health);
    }
}