using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float mouseSensitivity = 50f;

    private float xRotation = 0f;
    private Vector2 mouseDelta;

    void Start()
    {
        if (!player) Debug.LogError("No player found");
        transform.position = player.position;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse input from new Input System
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }
        
        Rotation();
    }

    private void Rotation()
    {
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        player.Rotate(Vector3.up * mouseX);

        mouseDelta = Vector2.zero;
    }
}