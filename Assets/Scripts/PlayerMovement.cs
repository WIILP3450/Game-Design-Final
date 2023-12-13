using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    // movement
    public float maxMoveSpeed;
    // cap how quickly a player can fall and prevent them from gaining speed
    private Vector2 respawnLocation;

    // jumping
    public float jumpForce;
    private float jumpTimeCounter;
    public float jumpTime;

    // player checks
    [SerializeField] public Transform groundCheck;
    [SerializeField] public Transform leftWallCheck;
    [SerializeField] public Transform rightWallCheck;
    [SerializeField] public float groundCheckRadius;
    [SerializeField] public float leftCheckRadius;
    [SerializeField] public float rightCheckRadius;
    [SerializeField] public LayerMask groundLayer;

    // dashing
    private Vector2 dashDirection;
    [SerializeField] private float dashVelocity;
    [SerializeField] public float dashTime;

    // climbing

    // counters
    public int deathCount = 0;
    public int applesCollected = 0;

    // private Animator animation;
    private Rigidbody2D rb;
    private Collider2D playerCollision; // collider for the deathplanes
    private Renderer playerRender;
    Animator animation;

    // booleans
    private bool hasDash;       // player can dash
    private bool isDashing;     // player is currently dashing
    private bool isGrounded;    // if player doesn't have a dash and they are grounded, refill dash
    private bool isAgainstLeftWall;
    private bool isAgainstRightWall;
    private bool isJumping;     // player is currently jumping
    [SerializeField] private bool active = true;    // determines if player is alive

    // player states for animation
    private enum AnimationStateEnum
    {
        Idle = 0,
        Running = 1,
        Jumping = 2,
        Falling = 3
    }

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();       // player rigidbody
        playerCollision = GetComponent<Collider2D>();   // deathplane/obstacles
        playerRender = GetComponent<SpriteRenderer>();
        animation = GetComponent<Animator>();

        SetRespawnPoint(transform.position);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!active)
        {
            // do nothing is player is currently dead
            return;
        }
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isAgainstLeftWall = Physics2D.OverlapCircle(leftWallCheck.position, leftCheckRadius, groundLayer);
        isAgainstRightWall = Physics2D.OverlapCircle(rightWallCheck.position, rightCheckRadius, groundLayer);

        // walking
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float VerticalInput = Input.GetAxisRaw("Vertical");

        // update movement velocity
        rb.velocity = new Vector2(horizontalInput * maxMoveSpeed, rb.velocity.y);

        // flip sprite when changing direction
        if (horizontalInput > 0)
        {
            //Debug.Log("now facing right");
            transform.localScale = new Vector3(1, 1, 1); // normal scale (no flip)
            animation.SetInteger("playerState", (int) AnimationStateEnum.Running);
        }
        else if (horizontalInput < 0)
        {
            //Debug.Log("now facing right");
            transform.localScale = new Vector3(-1, 1, 1); // flip horizontally
            animation.SetInteger("playerState", (int)AnimationStateEnum.Running);
        }
        else if (horizontalInput == 0 && isGrounded)
        {
            animation.SetInteger("playerState", (int)AnimationStateEnum.Idle);
        }

        // jumping
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            animation.SetInteger("playerState", (int)AnimationStateEnum.Jumping);
            //Debug.Log("jump triggered");
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        if(Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
                //animation.SetInteger("playerState", (int)AnimationStateEnum.Falling);
            }
        }
        if(Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }

        // dashing
        if (Input.GetKeyDown(KeyCode.C) && hasDash)
        {
            //Debug.Log("dash triggered");
            isDashing = true;
            hasDash = false;
            dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (dashDirection == Vector2.zero)
            {
                dashDirection = new Vector2(transform.localScale.x, 0);
            }
            StartCoroutine(StopDashing());  // calls coroutine to stop the dash
        }
        if (isDashing)
        {
            rb.velocity = dashDirection.normalized * dashVelocity;
        }

        // climbing
        if (isAgainstLeftWall && Input.GetKeyDown(KeyCode.A))
        {
            playerCollision.sharedMaterial.friction = 1;    // changes wall friction for walls to left of player
            rb.velocity = new Vector2(rb.velocity.x, VerticalInput * maxMoveSpeed);
            animation.SetInteger("playerState", (int)AnimationStateEnum.Idle );
        }
        else if (isAgainstRightWall && Input.GetKeyDown(KeyCode.D))
        {
            playerCollision.sharedMaterial.friction = 1;    // changes wall friction for walls to right of player
            rb.velocity = new Vector2(rb.velocity.x, VerticalInput * maxMoveSpeed);
            animation.SetInteger("playerState", (int)AnimationStateEnum.Idle);
        }

        // handle animations
        if (isGrounded)
        {
            hasDash = true;
        }
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ScreenTransition"))
        {
            //Debug.Log("triggered change respawn location");
            SetRespawnPoint(transform.position);
        }
        else if (collision.CompareTag("DashFlower"))
        {
            //Debug.Log("collected dash flower");
            // resets player's dash, even if they are in mid-air
            hasDash = true;
        }
        else if (collision.CompareTag("Apple"))
        {
            //Debug.Log("apple collected");
            applesCollected += 1;
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("EndofLevelChest"))
        {
            //Debug.Log("end of level reached");
            SceneManager.LoadScene("Main Menu");
        }
    }

    private void RespawnJump()
    {
        // used to prevent the player from endlessly respawning after a death
        animation.SetInteger("playerState", (int)AnimationStateEnum.Idle);
        rb.velocity = new Vector2(rb.velocity.x, jumpForce / 2);
    }
    public void Die()
    {
        active = false;
        rb.velocity = new Vector2 (0, 0);
        animation.SetInteger("playerState", (int)AnimationStateEnum.Falling);
        playerCollision.enabled = false;
        deathCount += 1;
        StartCoroutine(Respawn());
    }
    public void SetRespawnPoint(Vector2 position)
    {
        respawnLocation = position;
    }

    // IEnumerators
    private IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        //Debug.Log("dash stopped");
    }
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2f);    // wait time until respawn
        transform.position = respawnLocation;
        active = true;
        playerCollision.enabled = true;
        RespawnJump();
        //Debug.Log("respawned");
    }
}
