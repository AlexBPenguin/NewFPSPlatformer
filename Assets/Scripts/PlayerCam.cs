using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    PlayerInput playerInput;
    private PlayerControls controls;

    [Header("Sensitivity")]
    [SerializeField] float sensX;
    [SerializeField] float sensY;

    [Header("CamRotation")]
    float xRotation;
    float yRotation;

    [SerializeField] Transform orientation;
    [SerializeField] Transform camHolder;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        controls = new PlayerControls();
        controls.Gameplay.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        GetMouseInput();

        RotateCam();
    }


    void GetMouseInput()
    {
        Vector2 cameraInput = controls.Gameplay.CameraLook.ReadValue<Vector2>();
        //float mouseX = cameraInput.x * Time.deltaTime * sensX;
        //float mouseY = cameraInput.y * Time.deltaTime * sensY;

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
    }

    void RotateCam()
    {
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
