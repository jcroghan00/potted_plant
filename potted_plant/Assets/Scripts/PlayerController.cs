using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int health;
    public float moveSpeed;
    public float jumpSpeed;
    public int jumpLeft;
    public Vector2 climbJumpForce;
    public float fallSpeed;
    public float sprintSpeed;
    public float sprintTime;
    public float sprintInterval;
    public float attackInterval;

    public Color invulnerableColor;
    public Vector2 hurtRecoil;
    public float hurtTime;
    public float hurtRecoverTime;
    public Vector2 deathRecoil;
    public float deathDelay;

    public Vector2 attackUpRecoil;
    public Vector2 attackForwardRecoil;
    public Vector2 attackDownRecoil;

    public GameObject attackUpEffect;
    public GameObject attackForwardEffect;
    public GameObject attackDownEffect;

    private bool _isGrounded;
    private bool _isClimb;
    private bool _isSprintable;
    private bool _isSprintReset;
    private bool _isInputEnabled;
    private bool _isFalling;

    private float _climbJumpDelay = 0.2f;

    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private Transform _transform;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _isInputEnabled = true;
        _isSprintReset = true;

        _animator = gameObject.GetComponent<Animator>();
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _transform = gameObject.GetComponent<Transform>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _boxCollider = gameObject.GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        updatePlayerState();
        if (_isInputEnabled)
        {
            move();
            jump();
        }
    }

    // Constant clock update
    void FixedUpdate()
    {

    }

    public void jump()
    {
        if (!Input.GetButtonDown("Jump"))
            return;

        if (_isClimb)
            climbJump();
        else if (jumpLeft > 0)
            handleJump();
    }

    private void fall()
    {
        if (Input.GetButtonUp("Jump") && !_isClimb)
        {
            _isFalling = true;
            handleFall();
        } else
        {
            _isFalling = false;
        }
    }

    public void move()
    {
        // calculate movement
        float horizontalMovement = Input.GetAxis("Horizontal") * moveSpeed;

        // set velocity
        Vector2 newVelocity;
        newVelocity.x = horizontalMovement;
        newVelocity.y = _rigidbody.linearVelocity.y;
        _rigidbody.linearVelocity = newVelocity;

        if (!_isClimb)
        {
            // the sprite itself is inversed 
            float moveDirection = -transform.localScale.x * horizontalMovement;

            if (moveDirection < 0)
            {
                Vector3 oldScale = transform.localScale;
                // flip player sprite
                Vector3 newScale;
                newScale.x = horizontalMovement < 0 ? -oldScale.x : oldScale.x;
                newScale.y = oldScale.y;
                newScale.z = oldScale.z;

                transform.localScale = newScale;

                if (_isGrounded)
                {
                    // turn back animation
                    _animator.SetTrigger("IsRotate");
                }
            }
            else if (moveDirection > 0)
            {
                // move forward
                _animator.SetBool("IsRun", true);
            }
        }

        // stop
        if (Input.GetAxis("Horizontal") == 0)
        {
            _animator.SetTrigger("stopTrigger");
            _animator.ResetTrigger("IsRotate");
            _animator.SetBool("IsRun", false);
        }
        else
        {
            _animator.ResetTrigger("stopTrigger");
        }
    }

    private void updatePlayerState()
    {
        _isGrounded = checkGrounded();
        _animator.SetBool("IsGround", _isGrounded);

        float verticalVelocity = _rigidbody.linearVelocity.y;
        _animator.SetBool("IsDown", verticalVelocity < 0);

        if (_isGrounded && verticalVelocity == 0)
        {
            _animator.SetBool("IsJump", false);
            _animator.ResetTrigger("IsJumpFirst");
            _animator.ResetTrigger("IsJumpSecond");
            _animator.SetBool("IsDown", false);

            jumpLeft = 2;
            _isClimb = false;
            _isSprintable = true;
        }
        else if(_isClimb)
        {
            // one remaining jump chance after climbing
            jumpLeft = 1;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // enter climb state
        if (collision.collider.tag == "Wall" && !_isGrounded)
        {
            _rigidbody.gravityScale = 0;

            Vector2 newVelocity;
            newVelocity.x = 0;
            newVelocity.y = -2;

            _rigidbody.linearVelocity = newVelocity;

            _isClimb = true;
            _animator.SetBool("IsClimb", true);

            _isSprintable = true;
        }
        else {
            checkGrounded();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.tag == "Wall" && _isFalling && !_isClimb)
        {
            OnCollisionEnter2D(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // exit climb state
        if (collision.collider.tag == "Wall")
        {
            _isClimb = false;
            _animator.SetBool("IsClimb", false);

            _rigidbody.gravityScale = 1;
        }
    }

    private void handleJump() 
    {
        Vector2 newVelocity;
        newVelocity.x = _rigidbody.linearVelocity.x;
        newVelocity.y = jumpSpeed;

        _rigidbody.linearVelocity = newVelocity;

        _animator.SetBool("IsJump", true);
        jumpLeft -= 1;
        if (jumpLeft == 0)
        {
            _animator.SetTrigger("IsJumpSecond");
        } 
        else if (jumpLeft == 1)
        {
            _animator.SetTrigger("IsJumpFirst");
        }
    }

    private void climbJump()
    {
        Vector2 realClimbJumpForce;
        realClimbJumpForce.x = climbJumpForce.x * transform.localScale.x;
        realClimbJumpForce.y = climbJumpForce.y;
        _rigidbody.AddForce(realClimbJumpForce, ForceMode2D.Impulse);

        _animator.SetTrigger("IsClimbJump");
        _animator.SetTrigger("IsJumpFirst");

        _isInputEnabled = false;
        StartCoroutine(climbJumpCoroutine(_climbJumpDelay));
    }

    private void handleFall()
    {
        Vector2 newVelocity;
        newVelocity.x = _rigidbody.linearVelocity.x;
        newVelocity.y = -fallSpeed;

        _rigidbody.linearVelocity = newVelocity;
    }

    private IEnumerator climbJumpCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        _isInputEnabled = true;

        _animator.ResetTrigger("IsClimbJump");

        // jump to the opposite direction
        Vector3 newScale;
        newScale.x = -transform.localScale.x;
        newScale.y = 1;
        newScale.z = 1;

        transform.localScale = newScale;
    }

    private bool checkGrounded()
    {
        Vector2 origin = _transform.position;

        float radius = 0.19f;

        // detect downwards
        Vector2 direction;
        direction.x = 0;
        direction.y = -1;

        float distance = 2.44f;

        float originX = origin.x;
        float originY = origin.y - distance;

        Vector2 newOrigin = new Vector2(originX, originY);

        RaycastHit2D hitRec = Physics2D.CircleCast(newOrigin, radius, direction);

        if (hitRec.collider != null) {
            Debug.Log(hitRec.collider.gameObject.tag);
        }

        return hitRec.collider.gameObject.tag == "Platform";
    }

    /**
    public float gizmoDistance = 0.145f;
    public float gizmoRadius = 0.25f;

    void OnDrawGizmosSelected()
    {
        Vector2 origin = _transform.position;

        float radius = gizmoRadius;
        float distance = gizmoDistance;

        float originX = origin.x;
        float originY = origin.y - distance;

        Vector2 newOrigin = new Vector2(originX, originY);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newOrigin, radius);
    }
    */
}
