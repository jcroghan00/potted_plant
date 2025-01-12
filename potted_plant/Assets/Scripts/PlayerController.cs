using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public float speed = 1.0f;
    public float jumpStrength = 5.0f;
    public float airSpeed = 0.5f;

    private bool grounded = false;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;

    private PlayerInput input;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = gameObject.GetComponent<BoxCollider2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();

        input = gameObject.GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Constant clock update
    void FixedUpdate()
    {

    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("JUMP");
        if (grounded)
        {
            Vector2 jumpVelocity = new Vector3(0f, jumpStrength, 0);
            rb.linearVelocity += jumpVelocity;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 normalVector = new Vector2(1, 0);
        Debug.Log("MOVE");
        if (grounded) 
        {
            rb.linearVelocity = context.ReadValue<Vector2>() * normalVector * speed;
        }
        else 
        {
            rb.linearVelocity = context.ReadValue<Vector2>() * normalVector * speed * airSpeed;   
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            grounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            grounded = false;
        }
    }
}
