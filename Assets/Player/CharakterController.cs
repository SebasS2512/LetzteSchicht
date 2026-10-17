using UnityEngine;
using UnityEngine.InputSystem;

public class CharakterController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Schritt-Sound")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepDistance = 2.2f;

    private CharacterController controller;
    private Vector3 velocity;

    private float distanceWalked;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 input = ReadMovementInput();

        Vector3 move = transform.right * input.x + transform.forward * input.y;
        controller.Move(move.normalized * speed * Time.deltaTime);

        ApplyGravity();
        HandleFootsteps(input);
    }

    private Vector2 ReadMovementInput()
    {
        if (Keyboard.current == null) return Vector2.zero;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical = -1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical = 1f;

        return new Vector2(horizontal, vertical);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleFootsteps(Vector2 input)
    {
        if (!controller.isGrounded || input.sqrMagnitude < 0.01f)
        {
            distanceWalked = stepDistance * 0.5f;
            return;
        }

        distanceWalked += speed * Time.deltaTime;

        if (distanceWalked >= stepDistance)
        {
            distanceWalked = 0f;
            SoundUtils.PlayRandom(footstepSource, footstepClips);
        }
    }
}