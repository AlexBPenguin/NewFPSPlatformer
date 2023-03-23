using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    //BUG LIST
    //1.SUPER SPEED - seems to happen in air sometime after a platform jump
        //(in recent build jumping off the side of the platform instead of where it's going and looking off the side of the platform seems to activate bug. but not in the editor...)
        //its definetly the code that's trying to limit the player from going over their previous speed



    //Player Controls
    PlayerInput playerInput;
    PlayerControls controls;
    public Rigidbody rb;

    [Header("Old Input System Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;

    [Header("Input Values")]
    [SerializeField] float horizontalInput;
    [SerializeField] float verticalInput;
    Vector3 moveDirection;
    public Transform orientation;
    bool moveInput;
    bool forward;

    [Header("Grounded")]
    [SerializeField] bool isGrounded;
    public RaycastHit groundHit;
    public float playerHeight;
    public LayerMask whatIsGrounded;

    [Header("Basic Movement")]
    [SerializeField] float moveSpeed;
    [Header("In Air Movement")]
    [SerializeField] float airMultiplier;

    [Header("Speed Control")]
    [SerializeField] Vector3 flatVel;
    [SerializeField] float maxSpeed;
    [SerializeField] float slowSpeedMultiplier;
    [SerializeField] float groundSlowSpeed;
    [SerializeField] float airSlowSpeed;
    [SerializeField] float noInputAirSlowSpeed;
    [SerializeField] float groundDrag;
    [SerializeField] float desiredSpeed;
    float inAirSpeedCap;

    [Header("WallRun")]
    public float wallRunSpeed;
    public bool wallRunning;

    [Header("Jump")]
    [SerializeField] float jumpForce;
    [SerializeField] bool jumpPressed;
    bool jumping;

    [Header("Variable Jump")]
    [SerializeField] bool jumpKeyHeld;
    [SerializeField] int jumpHeldGravity;
    [SerializeField] int jumpNotHeldGravity;
    [SerializeField] Vector3 gravity;

    [Header("FallSpeed")]
    [SerializeField] Vector3 fallVel;
    [SerializeField] float maxFallSpeed;
    
    [Header("Coyote Time")]
    [SerializeField] float coyoteTime;
    [SerializeField] float coyoteCounter;
    [Header("Jump Buffer")]
    [SerializeField] float jumpBufferTime;
    [SerializeField] float jumpBufferCounter;

    [Header("MoveOnPlaform")]
    public bool onPlatform;
    //[SerializeField] GameObject movingPlatform;
    public GameObject movingPlatform;
    MovingPlatform mp;
    [SerializeField] float platformJumpForceMultiplier;
    bool platformJumping;
    //bool platformJumped;
    [Header("PlatformCoyoteTime")]
    [SerializeField] bool canPlatformJump;

    [Header("Text Stuff")]
    [SerializeField] TMP_Text text_Speed;
    [SerializeField] TMP_Text text_inAirSpeedCap;
    [SerializeField] TMP_Text text_MoveSpeed;
    [SerializeField] TMP_Text text_currentGravity;
    [SerializeField] TMP_Text text_currentSlowSpeedMultiplier;
    
    //[SerializeField] bool stopPlatform;
    //[SerializeField] bool platformReturned;
    //public float speed;
    //[SerializeField] GameObject movingPlatform;
    //[SerializeField] GameObject allOtherObjects;
    //public Vector3 platformDir;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        controls = new PlayerControls();
        controls.Gameplay.Enable();
        controls.Gameplay.Jump.performed += Jump_performed;
        controls.Gameplay.Jump.canceled += Jump_canceled;
        controls.Gameplay.Dash.performed += Dash_performed;

    }
    private void Jump_performed(InputAction.CallbackContext context)
    {
        Debug.Log("JumpButton");
        //set jump buffer
        jumpBufferCounter = jumpBufferTime;
        jumpPressed = true;
        
    }

    private void Jump_canceled(InputAction.CallbackContext context)
    {
        Debug.Log("JumpCancel");
        coyoteCounter = 0;
    }

    private void Dash_performed(InputAction.CallbackContext context)
    {
        Debug.Log("Dash");
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        //mp = movingPlatform.GetComponent<MovingPlatform>();
    }

    private void FixedUpdate()
    {
        if (!wallRunning)
        {
            MovePlayer();
        }
        
    }

    void Update()
    {
        MyInput();
        GroundCheck();
        JumpCheck();
        JumpBufferCheck();
        Gravity();
        SpeedControl();
        WallRun();
        MoveOnPlatform();
        TextStuff();
    }

    private void LateUpdate()
    {
        //inbetween next update, set variable to current velocity.magnitude
        //inAirSpeedCap = flatVel.magnitude;
        //Debug.Log("inaircap = vel");
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey))
        {
            //set jump buffer
            jumpBufferCounter = jumpBufferTime;
            jumpKeyHeld = true;
            jumpPressed = true;
        }

        if (Input.GetKeyUp(jumpKey))
        {
            jumpKeyHeld = false;
            coyoteCounter = 0;
        }
        //THIS IS FROM USING THE NEW INPUT SYSTEM WHICH SEEMS TO CAUSE A CAMERA JITTER ISSUE IN BUILDS
        /*
        //input system movement
        Vector2 inputVector = controls.Gameplay.Movement.ReadValue<Vector2>();
        //input values
        horizontalInput = inputVector.x;
        verticalInput = inputVector.y;
        */

        //check if any input
        if(verticalInput == 0 && horizontalInput == 0)
        {
            moveInput = false;
        }
        else
        {
            moveInput = true;
        }
        //check if moving forward
        if (verticalInput > 0)
        {
            forward = true;
        }
        else
        {
            forward = false;
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //idea is that force wont be added to platformjumps while/when jump occurs
        if (!platformJumping)
        {
            //on ground
            if (isGrounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10, ForceMode.Force);
                jumping = false;
                slowSpeedMultiplier = groundSlowSpeed;
            }

            //in air
            else if (!isGrounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10 * airMultiplier, ForceMode.Force);
            }
        }



    }

    private void SpeedControl()
    {
        flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (!platformJumping)
        {
            /*
            //check if speed has started to decrease in air(works on ground too without !isGrounded)
            if(flatVel.magnitude < inAirSpeedCap && !isGrounded)
            {
                Debug.Log("cant increase speed");
                cantIncreaseSpeed = true;
            }

            //check if current velocity is greater than inAirSpeed variable(set in late update)
            if(flatVel.magnitude > inAirSpeedCap && cantIncreaseSpeed && moveSpeed > maxSpeed && !isGrounded)
            {
                Debug.Log("Slow");
                moveSpeed = inAirSpeedCap;
            }*/

            //slow player in air when over max speed or when there's no input
            if (moveSpeed > maxSpeed || (moveInput == false && moveSpeed > 0))
            {
                moveSpeed -= Time.deltaTime * slowSpeedMultiplier;
            }

            //limit speed if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }

            if (moveInput)
            {
                //set speed if holding forward
                //also trying to make it so you can't turn around mid air while move speed is high
                if (forward && moveSpeed < maxSpeed || flatVel.magnitude < maxSpeed)
                {
                    moveSpeed = maxSpeed;
                }

                if (forward == false && moveSpeed < (maxSpeed + 1f))
                {
                    moveSpeed = (maxSpeed - 1f);
                }
            }

            //while in the air slow down
            if (!isGrounded)
            {
                //slow down faster with no input
                if (!moveInput)
                {
                    slowSpeedMultiplier = noInputAirSlowSpeed;
                }
                //in air slowspeed
                else
                {
                    slowSpeedMultiplier = airSlowSpeed;
                }
                

            }

            //handles drag
            if (isGrounded && jumping == false)
            {
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = 0f;
            }

            //stop player at speeds close to zero
            if (moveSpeed < 0.1f)
            {
                moveSpeed = 0;
            }
        }

        else if (platformJumping)
        {
            //limit platform speed gained to 30
            if(flatVel.magnitude <= 30)
            {
                desiredSpeed = flatVel.magnitude;
            }

            else
            {
                desiredSpeed = 30;
            }

            moveSpeed = desiredSpeed;
            //platformJumped = true;
        }
        
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out groundHit, playerHeight * 0.5f + 0.25f, whatIsGrounded);

        //check coyote time
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            //cantIncreaseSpeed = false;
            //platformJumped = false;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }

    void JumpBufferCheck()
    {
        jumpBufferCounter -= Time.deltaTime;
    }

    private void JumpCheck()
    {
        if (coyoteCounter > 0 && jumpBufferCounter > 0 && jumpPressed)
        {
            Debug.Log("JumpCheck");
            jumpPressed = false;
            Jump();
        }
    }

    private void Jump()
    {
        rb.drag = 0;
        jumping = true;
        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (!canPlatformJump)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        
        //moving platform jump
        else if (canPlatformJump)
        {
            platformJumping = true;
            rb.velocity = Vector3.zero;
            rb.AddForce(transform.up * jumpForce * 1.2f, ForceMode.Impulse);
            rb.AddForce(-mp.platformDir * (mp.platformSpeed * platformJumpForceMultiplier), ForceMode.Impulse);
            Invoke("PlatformJumpingFalse", 0.1f);
        }

        
    }

    private void Gravity()
    {
        gravity = Physics.gravity;

        //jump not held
        if (!isGrounded && (!jumpKeyHeld || rb.velocity.y < 5))
        {
            Physics.gravity = new Vector3(0, jumpNotHeldGravity, 0);
        }

        //jump held
        else if (!isGrounded && jumpKeyHeld)
        {
            Physics.gravity = Physics.gravity = new Vector3(0, jumpHeldGravity, 0);
        }

        fallVel = new Vector3(0f, rb.velocity.y, 0f);

        if (fallVel.y < maxFallSpeed)
        {
            //Vector3 limitFallVel = maxFallSpeed;
            rb.velocity = new Vector3(rb.velocity.x, maxFallSpeed, rb.velocity.z);
        }

    }

    private void WallRun()
    {
        if (wallRunning)
        {
            desiredSpeed = wallRunSpeed;
        }
    }

    private void MoveOnPlatform()
    {
        if(isGrounded && groundHit.transform.CompareTag("MovingPlatform") && !onPlatform)
        {
            movingPlatform = groundHit.transform.gameObject;
            //movingPlatform.transform.parent = null;
            mp = movingPlatform.GetComponent<MovingPlatform>();
            Debug.Log("OnPlatform");
            onPlatform = true;
            //canPlatformJump = true;
            Invoke(nameof(StartPlatform), 0.5f);
        }

        //believe this was stalling allotherobjects from moving when jumping off platform
        /*
        else if(coyoteCounter < 0)
        {
            onPlatform = false;
        }*/
        
        //if player is not on a platform and in the air
        else if(!isGrounded)
        {
            onPlatform = false;
            if (coyoteCounter < 0)
            {
                canPlatformJump = false;
            }
        }

        else if(isGrounded && !groundHit.transform.CompareTag("MovingPlatform"))
        {
            canPlatformJump = false;
        }

    }

    void StartPlatform()
    {
        //Debug.Log("MoveStopPlatform");
        //mp.stopPlatform = false;
        canPlatformJump = true;
        mp.StartPlatform();
    }

    void PlatformJumpingFalse()
    {
        platformJumping = false;
    }

    void TextStuff()
    {
        text_Speed.text = "Velocity: " + flatVel.magnitude.ToString();
        text_inAirSpeedCap.text = "InAirSpeedCap: " + inAirSpeedCap.ToString();
        text_MoveSpeed.text = "MoveSpeed: " + moveSpeed.ToString();
        text_currentGravity.text = "Gravity: " + gravity.y.ToString();
        text_currentSlowSpeedMultiplier.text = "SlowSpeed: " + slowSpeedMultiplier.ToString();
    }
}
