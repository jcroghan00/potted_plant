using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int health;
    public float moveSpeed;
    public float jumpSpeed;
    public int jumpLeft;
    public Vector2 climbJumpForce;
    public float fallSpeed;
    public float maxSwingDistance;
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
    private bool _isInputEnabled;
    private bool _isFalling;
    private bool _isSwinging;

    private float _climbJumpDelay = 0.2f;
    private float _gravityScale;
    private List<GameObject> _swingPoints;

    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private Transform _transform;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _isInputEnabled = true;

        _animator = gameObject.GetComponent<Animator>();
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _transform = gameObject.GetComponent<Transform>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _boxCollider = gameObject.GetComponent<BoxCollider2D>();

        _gravityScale = _rigidbody.gravityScale;
        _swingPoints = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        updatePlayerState();
        if (_isInputEnabled)
        {
            move();
            jump();
            fall();
            swing();
        }
    }

    void OnTriggerEnter2D(Collider2D c) 
    {
        _isGrounded = c.gameObject.tag == "Platform";
        jumpLeft = 2;
    }

    void OnTriggerExit2D(Collider2D c) 
    {
        if (c.gameObject.tag == "Platform") {
            _isGrounded = false;
        }
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
            //float moveDirection = -transform.localScale.x * horizontalMovement;

            Vector3 oldScale = transform.localScale;

            if (oldScale.x > 0 && horizontalMovement < 0) {
                // flip player sprite
                Vector3 newScale;
                newScale.x = -oldScale.x;
                newScale.y = oldScale.y;
                newScale.z = oldScale.z;

                transform.localScale = newScale;

                if (_isGrounded)
                {
                    // turn back animation
                    _animator.SetTrigger("IsRotate");
                }
            }
            else if (oldScale.x < 0 && horizontalMovement > 0) {
                // flip player sprite
                Vector3 newScale;
                newScale.x = -oldScale.x;
                newScale.y = oldScale.y;
                newScale.z = oldScale.z;

                transform.localScale = newScale;

                if (_isGrounded)
                {
                    // turn back animation
                    _animator.SetTrigger("IsRotate");
                }
            }

            // move forward
            _animator.SetBool("IsRun", true);
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

    public void swing() 
    {
        Debug.Log(_swingPoints.Count);
    }

    public void addSwingPoint(GameObject swingPoint) 
    {
        _swingPoints.Append(swingPoint);
    }

    private void updatePlayerState()
    {
        //_isGrounded = checkGrounded();
        _animator.SetBool("IsGround", _isGrounded);

        float verticalVelocity = _rigidbody.linearVelocity.y;
        _animator.SetBool("IsDown", verticalVelocity < 0);
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

            _rigidbody.gravityScale = _gravityScale;
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

        Vector3 oldScale = transform.localScale;
        // flip player sprite
        Vector3 newScale;
        newScale.x = -oldScale.x;
        newScale.y = oldScale.y;
        newScale.z = oldScale.z;

        transform.localScale = newScale;
    }
}
