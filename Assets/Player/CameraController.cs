using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float mouseSpeed = 100f;
    public Transform playerBody;
    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;
        if (UIController.Instance != null && UIController.Instance.IsNoteOpen()) return;

        float speed = mouseSpeed * GameSettings.MouseSensitivity;

        float mouseX = Mouse.current.delta.x.ReadValue() * speed * Time.deltaTime;
        float mouseY = Mouse.current.delta.y.ReadValue() * speed * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}