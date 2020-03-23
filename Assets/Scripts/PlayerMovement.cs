using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed;
    public float sensitivity;
    public Camera firstPersonCamera;

    CharacterController characterController;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float forward = Input.GetAxis("Vertical") * 0.05f;
        float side = Input.GetAxis("Horizontal") * 0.05f;

        Vector3 movement = transform.forward * forward + transform.right * side;
        characterController.Move(movement * movementSpeed);

        float h = sensitivity * Input.GetAxis("Mouse X");
        float v = sensitivity * -Input.GetAxis("Mouse Y");

        transform.Rotate(0, h, 0);
        firstPersonCamera.transform.Rotate(v, 0, 0);
    }
}
