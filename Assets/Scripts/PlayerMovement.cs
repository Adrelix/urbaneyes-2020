using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed;
    public float sensitivity;
    public Camera firstPersonCamera;

    Vector3 velocity = new Vector3(0, 0, 0);

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
        float forward = Input.GetAxis("Vertical");
        float side = Input.GetAxis("Horizontal");

	Vector3 m = new Vector3(forward, 0, side);
	Vector3 norm = Vector3.Normalize(m);

        Vector3 movement = norm.x * transform.forward + norm.z * transform.right;
        characterController.Move(movement * movementSpeed * Time.deltaTime);

        float h = sensitivity * Input.GetAxis("Mouse X");
        float v = sensitivity * -Input.GetAxis("Mouse Y");

	if (characterController.isGrounded) {
		velocity.y = 0;
	} else {
		velocity += Physics.gravity * Time.deltaTime;
	}
	
	characterController.Move(velocity);

        transform.Rotate(0, h, 0);
        firstPersonCamera.transform.Rotate(v, 0, 0);
    }
}
