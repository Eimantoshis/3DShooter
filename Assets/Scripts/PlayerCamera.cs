using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform player;

    [SerializeField] private float mouseSensitivity = 200f;

    private float xRotation = 0f;
    // Start is called before the first frame update
    void Start() {
        if (!player) Debug.LogError("No player found");
        transform.position = player.position;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Rotation();
    }

    private void Rotation() {
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // prevent over-rotation
        
        // up/down rotation
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        player.Rotate(UnityEngine.Vector3.up * mouseX);
    }
    
}
