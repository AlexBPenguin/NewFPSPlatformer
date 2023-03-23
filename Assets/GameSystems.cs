using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameSystems : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] Transform respawnPoint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            player.GetComponent<PlayerMovement>().rb.velocity = Vector3.zero;
            player.GetComponent<PlayerMovement>().transform.position = respawnPoint.position;
        }
    }
}
