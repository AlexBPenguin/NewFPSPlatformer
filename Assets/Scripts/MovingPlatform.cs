using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovingPlatform : MonoBehaviour
{
    //BUG LIST
    //1. OTHER PLATFORM MOVES AWAY FROM WAYPOINT (the other platform likely moves along with the other, as if being called on simultaneously)

    [Header("Waypoints")]
    [SerializeField] Transform wayPointA;
    [SerializeField] Transform wayPointB;

    [Header("References")]
    [SerializeField] GameObject player;
    PlayerMovement pm;
    [SerializeField] GameObject allOtherObjects;
    GameObject currentPlatformObject;

    [Header("Variables")]
    public float platformSpeed;
    [SerializeField] float returnPlatformSpeed;
    [SerializeField] float platformAccelerateSpeed;

    public Vector3 platformDir;
    public bool stopPlatform;
    bool returned;

    private void Awake()
    {
        transform.position = wayPointA.position;

        platformDir = (wayPointA.position - wayPointB.position).normalized;

        transform.forward = platformDir;
    }

    private void Start()
    {
        pm = player.GetComponent<PlayerMovement>();
        stopPlatform = true;
        returned = true;
    }

    private void Update()
    {
        if(transform.gameObject == pm.movingPlatform)
        {
            if (!stopPlatform)
            {
                if (returned)
                {
                    platformSpeed += Time.deltaTime * platformAccelerateSpeed;
                }

                if (pm.onPlatform)
                {
                    transform.parent = null;
                    allOtherObjects.transform.Translate(platformDir * platformSpeed * Time.deltaTime, Space.World);
                }

                if (!pm.onPlatform)
                {
                    transform.parent = allOtherObjects.transform;
                    gameObject.transform.Translate(-platformDir * platformSpeed * Time.deltaTime, Space.World);
                }
            }
        }

        else
        {
            if (!stopPlatform)
            {
                gameObject.transform.Translate(-platformDir * platformSpeed * Time.deltaTime, Space.World);
            }
        }
        /*
        if (!stopPlatform)
        {
            if (returned)
            {
                platformSpeed += Time.deltaTime * platformAccelerateSpeed;
            }

            if (pm.onPlatform)
            {
                transform.parent = null;
                allOtherObjects.transform.Translate(platformDir * platformSpeed * Time.deltaTime, Space.World);
            }

            if(!pm.onPlatform)
            {
                transform.parent = allOtherObjects.transform;
                gameObject.transform.Translate(-platformDir * platformSpeed * Time.deltaTime, Space.World);
            }

            
            
        }*/

        //stop at waypointB (calls only once it happens)
        if ((Vector3.Distance(transform.position, wayPointB.position) < 0.5f) && returned)
        {
            stopPlatform = true;
            returned = false;
            //I believe this is so coyote time can still be used without setting platform speed to zero
            Invoke(nameof(PlatformSpeedZero), 0.15f);
            Invoke(nameof(ReturnPlatform), 0.5f);
        }

        //stop at waypointA (calls only once it happens)
        if((Vector3.Distance(transform.position, wayPointA.position) < 0.5f) && !returned)
        {
            stopPlatform=true;
            returned = true;

            //for now, likely need fixing later
            platformDir = -platformDir;
            //platformSpeed = platformSpeed * 2;
            platformSpeed = 0f;

            if (pm.onPlatform)
            {
                Invoke(nameof(StartPlatform), 0.25f);
            }
        }
    }

    void PlatformSpeedZero()
    {
        platformSpeed = 0f;
    }

    public void StartPlatform()
    {
        stopPlatform = false;
    }

    void ReturnPlatform()
    {
        platformDir = -platformDir;
        //platformSpeed = platformSpeed / 2;
        platformSpeed = returnPlatformSpeed;
        stopPlatform = false;
    }
}
