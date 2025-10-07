using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {

    [SerializeField] private float speed = 25f;
    [SerializeField] private float jumpForce = 25f;
    
    private Rigidbody rb;
    
    
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference jump;

    private Vector2 moveDir;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        Move();
    }

    private void OnEnable() {
        jump.action.started += Jump;
    }

    private void OnDisable() {
        jump.action.started -= Jump;
    }
    

    private void Jump(InputAction.CallbackContext context) {
        Debug.Log("Jump");
    }

    private void Move() {
        moveDir = move.action.ReadValue<Vector2>();

        Vector3 dir = transform.right * moveDir.x + transform.forward * moveDir.y;
        rb.velocity = new Vector3(dir.x * speed, rb.velocity.y, dir.z * speed);
    }
    
}
