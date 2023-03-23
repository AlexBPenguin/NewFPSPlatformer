using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private RaycastHit againstWallHit;
    private bool wallLeft;
    private bool wallRight;
    public bool againstWall;
    public bool canLeaveWall;

    public Vector3 wallNormalPublic;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.wallRunning)
        {
            WallRunningMovement();
        }
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        //returns AboveGround bool as true if the raycast below hits nothing
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        //getting inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // state 1 - wallrunning
        //if against wall to left or right and player is inputing forward and player is above ground...
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
        {
            //start wallrun
            if (!pm.wallRunning)
            {
                StartWallRun();
            }
        }

        /*else
         {
             if (pm.wallRunning)
             {
                 StopWallRun();
             }
         }*/

        //if(pm.wallRunning && canLeaveWall && (verticalInput <= 0 || !AboveGround() || !againstWall))
        if (pm.wallRunning && canLeaveWall && (verticalInput <= 0 || !AboveGround() || !againstWall))
        {
            StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.wallRunning = true;
        wallNormalPublic = wallRight ? rightWallHit.normal : leftWallHit.normal;
    }

    private void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        //if wall is on the right, use rightwallHit, otherwise wall must be onthe left so use leftWallHit
        //Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        //use cross product of wallNormal(vector pointing away from wall towards player)
        //and the up direction to find wall's forward direction.
        //I think this makes it so walls can't be tilted since transform.up is used and not something specific for the wall
        Vector3 wallForward = Vector3.Cross(wallNormalPublic, transform.up);

        //checks to see where orientation is facing and gives proper wall move direction
        if((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = - wallForward;
        }

        againstWall = Physics.Raycast(transform.position, -wallNormalPublic, out againstWallHit, wallCheckDistance, whatIsWall);
        Debug.DrawRay(transform.position, -wallNormalPublic, Color.red);

        //forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //push player towards wall if not inputting away from wall
        if(!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        {
            rb.AddForce(-wallNormalPublic * 100, ForceMode.Force);
        }

        canLeaveWall = true;
    }

    private void StopWallRun()
    {
        pm.wallRunning = false;
        rb.useGravity = true;
        canLeaveWall = false;
    }
}
